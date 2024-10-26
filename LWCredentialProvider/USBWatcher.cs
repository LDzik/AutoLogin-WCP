using System;
using System.Management;

public class USBWatcher
{
    private ManagementEventWatcher watcher;

    public void StartWatching()
    {
        WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2");
        watcher = new ManagementEventWatcher(query);
        watcher.EventArrived += new EventArrivedEventHandler(OnUSBInserted);
        watcher.Start();
    }

    private void OnUSBInserted(object sender, EventArrivedEventArgs e)
    {
        Console.WriteLine("USB inserted event detected.");
        //StopWatching();
    }

    public void StopWatching()
    {
        watcher.Stop();
    }
}