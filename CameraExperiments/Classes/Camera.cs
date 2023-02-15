using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CameraExperiments
{
    internal class Camera
    {
        public string? FriendlyName { get; set; }
        public CameraType Type { get; set; }
        public string? WifiSSID { get; set; }
        public string? WifiPassword { get; set; }
        public bool? BluetoothActivate { get; set; }
        public string? BluetoothDeviceId { get; set; }
        public string? HostNameOrIPOverride { get; set; }
        public bool? DeleteFiles { get; set; }
        public string? RemoteFolder { get; set; }
        public bool? Enabled { get; set; }
        public bool? DiscardFootageBetweenCertainTimes { get; set; }

        /* Times in HH:mm:ss format */
        public string? DiscardFootageBetweenCertainTimesStart { get; set; }
        public string? DiscardFootageBetweenCertainTimesEnd { get; set; }
    }
}
