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
    }

    internal class CamerasList
    {
        public List<Camera> cameras { get; set; }
    }
}
