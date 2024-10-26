using System;
using System.Management;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

public class USBManager
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool DeviceIoControl(
        SafeFileHandle hDevice,
        uint dwIoControlCode,
        IntPtr lpInBuffer,
        uint nInBufferSize,
        IntPtr lpOutBuffer,
        uint nOutBufferSize,
        out uint lpBytesReturned,
        IntPtr lpOverlapped);

    private const uint IOCTL_DISK_GET_DRIVE_GEOMETRY = 0x00070000;
    
    [StructLayout(LayoutKind.Sequential)]
    public struct DISK_GEOMETRY
    {
        public long Cylinders;
        public int MediaType;
        public int TracksPerCylinder;
        public int SectorsPerTrack;
        public int BytesPerSector;
    }

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern SafeFileHandle CreateFile(
        string lpFileName,
        FileAccess dwDesiredAccess,
        FileShare dwShareMode,
        IntPtr lpSecurityAttributes,
        FileMode dwCreationDisposition,
        int dwFlagsAndAttributes,
        IntPtr hTemplateFile
    );

    public static string ReadUSBKey(string devicePath, int originalSize)
    {
        using (SafeFileHandle hDevice = OpenDevice(devicePath, FileAccess.Read))
        {
            if (hDevice.IsInvalid)
            {
                int errorCode = Marshal.GetLastWin32Error();
                Console.WriteLine($"Error opening device: {errorCode}");
                return string.Empty;
            }

            DISK_GEOMETRY dg = new DISK_GEOMETRY();
            uint junk;
            int blockSize;

            IntPtr dgBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(dg));
            try
            {
                if (!DeviceIoControl(hDevice, IOCTL_DISK_GET_DRIVE_GEOMETRY, IntPtr.Zero, 0, dgBuffer, (uint)Marshal.SizeOf(dg), out junk, IntPtr.Zero))
                {
                    Console.WriteLine($"Failed to get drive geometry. Error: {Marshal.GetLastWin32Error()}");
                    return string.Empty;
                }

                dg = Marshal.PtrToStructure<DISK_GEOMETRY>(dgBuffer);
                blockSize = dg.BytesPerSector;
                Console.WriteLine($"Block size: {blockSize}");
            }
            finally
            {
                Marshal.FreeHGlobal(dgBuffer);
            }

            int paddedSize = ((originalSize + blockSize - 1) / blockSize) * blockSize;
            byte[] buffer = new byte[paddedSize];

            using (FileStream fs = new FileStream(hDevice, FileAccess.Read))
            {
                int bytesRead = fs.Read(buffer, 0, paddedSize);
                Console.WriteLine($"USBKey read {bytesRead} bytes.");
            }

            return System.Text.Encoding.UTF8.GetString(buffer, 0, originalSize);
        }
    }

    private static SafeFileHandle OpenDevice(string devicePath, FileAccess access)
    {
        return CreateFile(devicePath, access, FileShare.Read, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
    }

    public static List<string> GetUSBDrive(){

        var removableDrives = new List<string>();
        var searcher = new ManagementObjectSearcher("SELECT DeviceID, MediaType, Caption FROM Win32_DiskDrive WHERE MediaType = 'Removable Media'");

        foreach (ManagementObject disk in searcher.Get())
        {
            string deviceID = disk["DeviceID"].ToString();
            //string caption = disk["Caption"].ToString();
            //Console.WriteLine($"Found USB drive: {caption} ({deviceID})");
            //return deviceID;
            removableDrives.Add(deviceID);
        }

        return removableDrives;
    }


    public static bool CheckUSBKey(string usbKey)
    {

        if (string.IsNullOrEmpty(usbKey))
            return false;


        var keyParts = usbKey.Split('-');

        Console.WriteLine($"key parts length: {keyParts.Length}");

        if (keyParts.Length != 5)
            return false;

        string prefix = keyParts[0];
        string date = $"{keyParts[1]}-{keyParts[2]}";
        string hash = keyParts[3];
        string guid = keyParts[4];

        Console.WriteLine($"prefix: {prefix}");
        Console.WriteLine($"date: {date}");



        // Validate date format
        if (!System.Text.RegularExpressions.Regex.IsMatch(date, @"^\d{8}-\d{6}$"))
            return false;

        Console.WriteLine($"hash: {hash}");


        // Validate hash format
        if (!System.Text.RegularExpressions.Regex.IsMatch(hash, @"^[0-9A-F]{10}$"))
            return false;

        Console.WriteLine($"guid: {guid}");

        // Validate GUID format
        if (!System.Text.RegularExpressions.Regex.IsMatch(guid, @"^[0-9A-F]{1,32}$"))
            return false;


        string hashInput = prefix + guid;
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(hashInput));
        string hashCheck = BitConverter.ToString(bytes).Replace("-", "").ToUpperInvariant().Substring(0, 10);

        Console.WriteLine($"hashCheck: {hashCheck}");
        if (hash != hashCheck)
            return false;

        return true;

    }
}