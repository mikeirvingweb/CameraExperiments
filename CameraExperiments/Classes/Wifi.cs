using SimpleWifi;
using System.ComponentModel;
using System.Text.Json;

namespace CameraExperiments
{
    internal class WifiService
    {
        Wifi wifi = new();

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

        public bool WifiConnect(Camera? camera)
        {
            bool success = false;
            int sleepOnCompletion = 10;

            string ssid = "", password = "";

            if (camera == null)
            {
                var settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText("Settings.json"))!;

                ssid = settings.localWifiSSID;
                password = settings.localWifiPassword;
            }
            else
            {
                ssid = camera.WifiSSID;
                password = camera.WifiPassword;
            }

            Console.WriteLine("Connecting Wifi Network: " + ssid);

            IEnumerable<AccessPoint> accessPoints = wifi.GetAccessPoints();

            foreach (AccessPoint ap in accessPoints)
            {
                if (ap.Name == ssid)
                {
                    AuthRequest authRequest = new AuthRequest(ap) { Password = password };

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
                                catch (Win32Exception e)
                                {
                                    success = false;
                                }
                            }

                            if (success)
                            {
                                Console.WriteLine("Wifi connected - " + ssid);
                            }
                        }
                    }
                }
            }

            if (success && sleepOnCompletion > 0)
            {
                Console.Write("Pause after Wifi Connection");

                for (var i = 0; i < sleepOnCompletion; i++)
                {
                    Thread.Sleep(1000);
                    Console.Write(".");
                }

                Console.WriteLine(Environment.NewLine);
            }

            return success;
        }

        public bool WifiConnectAttempts(Camera? camera, int numberOfAttempts)
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

        public bool WifiIsConnectedTo(string ssid)
        {
            var connected = false;
            
            IEnumerable<AccessPoint> accessPoints = wifi.GetAccessPoints();

            foreach (AccessPoint ap in accessPoints)
            {
                if (ap.Name == ssid && ap.IsConnected)
                {
                    connected = true;
                }
            }

            return connected;
        }
    }
}
