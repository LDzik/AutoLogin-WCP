using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

public class UserManager
{
    public const string Separator = "\u2561";

    public string DecodeData(string encodedDataHex, string keyHex, string ivHex)
    {
        if (string.IsNullOrEmpty(encodedDataHex))
            return "";

        byte[] encodedData = HexStringToByteArray(encodedDataHex);
        byte[] key = HexStringToByteArray(keyHex);
        byte[] iv = HexStringToByteArray(ivHex);

        using Aes aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        string decryptedText;
        using (var msDecrypt = new MemoryStream(encodedData))
        {
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt, Encoding.UTF8, true);
            decryptedText = srDecrypt.ReadToEnd();
        }
        
        return decryptedText;
    }


    public string EncodeData(string username, string password, string keyHex, string ivHex)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            return "";

        string dataText = username + Separator + password;

        byte[] key = HexStringToByteArray(keyHex);
        byte[] iv = HexStringToByteArray(ivHex);
        byte[] dataBytes = Encoding.UTF8.GetBytes(dataText);

        using Aes aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        byte[] encryptedBytes;

        using (var msEncrypt = new MemoryStream())
        {
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                csEncrypt.Write(dataBytes, 0, dataBytes.Length);
                csEncrypt.FlushFinalBlock();
            }
            encryptedBytes = msEncrypt.ToArray();
        }

        return BitConverter.ToString(encryptedBytes).Replace("-", "");
    }

    private byte[] HexStringToByteArray(string hex)
    {
        return Enumerable.Range(0, hex.Length)
                         .Where(x => x % 2 == 0)
                         .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                         .ToArray();
    }
}