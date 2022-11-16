using InTheHand.Bluetooth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Media.Capture;

namespace CameraExperiments
{
    internal enum CameraType
    {
        Ceyomur
    }

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
    }

    internal class CameraDetails
    {
        public CameraType Type { get; set; }
        public string? HostNameOrIP { get; set; }
        public bool? Secure { get; set; }
        public string? ListPath { get; set; }
        public string? FetchPath { get; set; }
        public string? DeletePath { get; set; }
    }
}
