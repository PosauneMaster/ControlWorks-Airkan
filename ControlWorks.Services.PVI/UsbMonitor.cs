using System;
using System.Diagnostics;
using System.Management;

namespace ControlWorks.Services.PVI
{
    public enum EventType
    {
        Inserted = 2,
        Removed = 3
    }

    internal class UsbMonitor
    {
        public event EventHandler<UsbMonitorArgs> DriveChanged;

        protected void OnDriveChanged(UsbMonitorArgs args)
        {
            var temp = DriveChanged;
            if (temp != null)
            {
                temp(this, args);
            }

        }

        public void Run()
        {
            ManagementEventWatcher watcher = new ManagementEventWatcher();
            WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2 or EventType = 3");

            watcher.EventArrived += (s, e) =>
            {
                string driveName = e.NewEvent.Properties["DriveName"].Value.ToString();
                EventType eventType = (EventType)(Convert.ToInt16(e.NewEvent.Properties["EventType"].Value));

                string eventName = Enum.GetName(typeof(EventType), eventType);

                Trace.TraceInformation($"USB Monitor: {eventName}");

                OnDriveChanged(new UsbMonitorArgs(eventType, driveName));

            };

            watcher.Query = query;
            watcher.Start();

            Console.ReadKey();
        }
    }

    public class UsbMonitorArgs : EventArgs
    {
        public EventType EventType { get; set; }
        public string DriveName { get; set; }

        public UsbMonitorArgs(EventType eventType, string driveName)
        {
            EventType = eventType;
            DriveName = driveName;
        }
    }
}