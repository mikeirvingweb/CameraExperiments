using SimpleWifi;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace CameraExperiments
{
    internal class WifiService
    {
        Wifi wifi = new();

        public bool WifiAvailable()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return true;
            }
            else
            {
                return !wifi.NoWifiAvailable;
            }
        }

        public void WifiDisconnect()
        {
            Console.WriteLine("Disconnecting from current Wifi Network.");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                try
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo()
                    {
                        FileName = "nmcli",
                        Arguments = "d disconnect wlan0",
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };

                    Process proc = new Process() { StartInfo = startInfo };
                    proc.Start();

                    string output = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit();

                    //Console.WriteLine(output);
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                if (wifi.ConnectionStatus == WifiStatus.Connected)
                    wifi.Disconnect();
            }                    
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

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                try
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo()
                    {
                        FileName = "nmcli",
                        Arguments = "d wifi connect " + ssid + " password " + password,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    
                    Process proc = new Process() { StartInfo = startInfo };
                    proc.Start();

                    string output = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit();

                    //Console.WriteLine(output);

                    if(output.ToLower().Contains("successfully activated"))
                    {
                        success = true;
                    }
                }
                catch (Exception e)
                {

                }
            }
            else
            {
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
                            }
                        }
                    }
                }
            }

            if (success)
            {
                Console.WriteLine("Wifi connected - " + ((camera == null)? "Primary / Local Wifi" : ssid));
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
