using System;
using System.Management;
using Lithnet.CredentialProvider;

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
        

        // CredentialProviderBase wcpBase = new MyCredentialProvider();

        // CredentialTile2 myTile = new MyTile(wcpBase);
        
        // wcpBase.SetDefaultTile(myTile, true);

        // wcpBase.ReloadUserTiles();
      
        // Initialize();
        // GetCredentials();
    }


    public void StopWatching()
    {
        watcher.Stop();
    }
}