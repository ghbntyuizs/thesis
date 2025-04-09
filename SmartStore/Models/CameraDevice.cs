namespace SmartStorePOS.Models
{
    public class CameraDevice
    {
        public string Name { get; set; }
        public string DeviceId { get; set; }
        public string DevicePath { get; set; }
        public string IpUrl { get; set; }

        public CameraDevice(string name, string deviceId, string devicePath, string ipUrl = null)
        {
            Name = name;
            DeviceId = deviceId;
            DevicePath = devicePath;
            IpUrl = ipUrl;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
