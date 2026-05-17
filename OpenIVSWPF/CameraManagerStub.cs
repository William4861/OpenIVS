using System;
using System.Collections.Generic;
using System.Drawing;

namespace DLCV.Camera
{
    public class DeviceInfoWrapper
    {
        public string ManufacturerName { get; set; }
        public string ModelName { get; set; }
        public string SerialNumber { get; set; }
        public string DeviceType { get; set; }
        public string UserId { get; set; }
    }

    public class ImageEventArgs : EventArgs
    {
        public Bitmap Image { get; }
        public ImageEventArgs(Bitmap image) => Image = image;
    }

    public static class TriggerConfig
    {
        public enum TriggerMode { Off, Software, Line0, Line1, Line2, Line3, Counter }
    }

    public class CameraDevice
    {
        public bool IsOpen => false;
    }

    public class CameraManager : IDisposable
    {
#pragma warning disable CS0067
        public event EventHandler<ImageEventArgs> ImageUpdated;
#pragma warning restore CS0067

        public CameraDevice ActiveDevice => null;
        public List<DeviceInfoWrapper> RefreshDeviceList() => new List<DeviceInfoWrapper>();
        public bool ConnectDevice(int index) => false;
        public void SetTriggerMode(TriggerConfig.TriggerMode mode) { }
        public bool StartGrabbing() => false;
        public void StopGrabbing() { }
        public void TriggerOnce() { }
        public void DisconnectDevice() { }
        public void Dispose() { }
    }
}
