using SimpleWifi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraExperiments
{
    internal class WifiService
    {
        Wifi wifi = new Wifi();

        public bool WifiAvailable()
        {
            return !wifi.NoWifiAvailable;
        }

        public void WifiDisconnect()
        {
            Console.WriteLine("Disconnecting from current Wifi Network.");

            if (wifi.ConnectionStatus == WifiStatus.Connected)
                wifi.Disconnect();
        }

        public bool WifiConnect(Camera camera)
        {
            bool success = false;

            Console.WriteLine("Connecting Wifi Network.");

            IEnumerable<AccessPoint> accessPoints = wifi.GetAccessPoints();

            foreach (AccessPoint ap in accessPoints)
            {
                if (ap.Name == camera.WifiSSID)
                {
                    AuthRequest authRequest = new AuthRequest(ap) { Password = camera.WifiSSID };

                    if (!ap.IsConnected)
                    {
                        if (authRequest.IsPasswordRequired)
                        {
                            if (ap.HasProfile)
                            {
                                success = ap.Connect(authRequest);
                            }
                            else
                            {
                                try
                                {
                                    success = ap.Connect(authRequest, false);
                                }
                                catch (Win32Exception)
                                {
                                    success = false;
                                }
                            }

                            if (success)
                            {
                                Console.WriteLine("Wifi connected - " + camera.WifiSSID);
                            }
                        }
                    }
                }
            }

            return success;
        }

        public bool WifiConnectAttempts(Camera camera, int numberOfAttempts)
        {
            int attempts = 0;
            bool success = false;

            do
            {
                success = WifiConnect(camera);
                attempts++;

                if (!success)
                {
                    Thread.Sleep(10 * 1000);
                }
            }
            while ((attempts < numberOfAttempts) && !success);

            return success;
        }
    }
}
