using SimpleWifi;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
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
            Console.WriteLine("Disconnecting from current Wifi Network.\n");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                try
                {
                    StringBuilder output = new StringBuilder();
                    StringBuilder error = new StringBuilder();

                    using (Process process = new Process())
                    {
                        var timeout = 15000;

                        ProcessStartInfo startInfo = new ProcessStartInfo()
                        {
                            FileName = "nmcli",
                            Arguments = "d disconnect wlan0",
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            WindowStyle = ProcessWindowStyle.Hidden
                        };

                        process.StartInfo = startInfo;

                        using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                        using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                        {
                            process.OutputDataReceived += (sender, e) => {
                                if (e.Data == null)
                                {
                                    outputWaitHandle.Set();
                                }
                                else
                                {
                                    output.AppendLine(e.Data);
                                }
                            };
                            process.ErrorDataReceived += (sender, e) =>
                            {
                                if (e.Data == null)
                                {
                                    errorWaitHandle.Set();
                                }
                                else
                                {
                                    error.AppendLine(e.Data);
                                }
                            };

                            process.Start();

                            process.BeginOutputReadLine();
                            process.BeginErrorReadLine();

                            if (process.WaitForExit(timeout) &&
                                outputWaitHandle.WaitOne(timeout) &&
                                errorWaitHandle.WaitOne(timeout))
                            {
                                // Process completed. Check process.ExitCode here.
                            }
                            else
                            {
                                // Timed out.
                            }
                        }
                    }

                    //Console.WriteLine(output);
                    //Console.WriteLine(error);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
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

            Console.WriteLine("Connecting Wifi Network: " + ((camera == null) ? "Primary / Local Wifi" : ssid.Substring(0, 3) + "*..."));

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                try
                {
                    StringBuilder output = new StringBuilder();
                    StringBuilder error = new StringBuilder();

                    using (Process process = new Process())
                    {
                        var timeout = 15000;

                        ProcessStartInfo startInfo = new ProcessStartInfo()
                        {
                            FileName = "Scripts/WifiLinux.sh",
                            Arguments = ssid + " " + password,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            WindowStyle = ProcessWindowStyle.Hidden
                        };

                        process.StartInfo = startInfo;

                        using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                        using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                        {
                            process.OutputDataReceived += (sender, e) => {
                                if (e.Data == null)
                                {
                                    outputWaitHandle.Set();
                                }
                                else
                                {
                                    output.AppendLine(e.Data);
                                }
                            };
                            process.ErrorDataReceived += (sender, e) =>
                            {
                                if (e.Data == null)
                                {
                                    errorWaitHandle.Set();
                                }
                                else
                                {
                                    error.AppendLine(e.Data);
                                }
                            };

                            process.Start();

                            process.BeginOutputReadLine();
                            process.BeginErrorReadLine();

                            if (process.WaitForExit(timeout) &&
                                outputWaitHandle.WaitOne(timeout) &&
                                errorWaitHandle.WaitOne(timeout))
                            {
                                // Process completed. Check process.ExitCode here.
                            }
                            else
                            {
                                // Timed out.
                            }
                        }
                    }

                    //Console.WriteLine(output);
                    //Console.WriteLine(error);

                    if (output.ToString().ToLower().Contains("successfully activated"))
                    {
                        success = true;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
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

            Console.WriteLine("Wifi Connection " + (success ? "Successful." : "Unsuccessful.\n"));

            if (success)
            {
                if (sleepOnCompletion > 0)
                {
                    Console.Write("Pause after Wifi Connection");

                    for (var i = 0; i < sleepOnCompletion; i++)
                    {
                        Thread.Sleep(1000);
                        Console.Write(".");
                    }

                    Console.WriteLine(Environment.NewLine);
                }
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
