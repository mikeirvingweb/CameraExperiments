#if DEBUG
using InTheHand.Bluetooth;
#endif

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace CameraExperiments
{
    internal class BluetoothService
    {
        public async Task<bool> BluetoothWakeupAsync(Camera camera)
        {
            bool awoken = false;

            if (camera.BluetoothActivate != true)
                return awoken;

            string? deviceId;
            string? serviceUuidStr;
            string? sendCommand;
            int sleepOnCompletion = 0;

            if (camera.Type == CameraType.Ceyomur)
            {
                deviceId = camera.BluetoothDeviceId;
                serviceUuidStr = "0000ffe0-0000-1000-8000-00805f9b34fb";
                sendCommand = "GPIO3";
                sleepOnCompletion = 15;
            }
            else
            {
                return false;
            }

            Console.WriteLine("Attempting Bluetooth Activation.");

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                try
                {
                    StringBuilder output = new StringBuilder();
                    StringBuilder error = new StringBuilder();

                    using (Process process = new Process())
                    {
                        var timeout = 30000;

                        ProcessStartInfo startInfo = new ProcessStartInfo()
                        {
                            FileName = "Scripts/BluetoothLinux.sh",
                            Arguments = DataHelper.IdToMacAddress(deviceId) + " " + serviceUuidStr + " " + Convert.ToHexString(Encoding.UTF8.GetBytes(sendCommand)),
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

                    if (output.ToString().ToLower().Contains("written successfully"))
                    {
                        awoken = true;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
#if DEBUG
                BluetoothUuid? serviceUuid = Guid.Parse(serviceUuidStr);

                try // awake by specific id, if it can be seen
                {
                    var bd = await BluetoothDevice.FromIdAsync(deviceId);

                    if (bd != null)
                    {
                        var serv = await bd.Gatt.GetPrimaryServiceAsync((BluetoothUuid)serviceUuid);

                        if (serv != null)
                        {
                            Thread.Sleep(5000);

                            var c = await serv.GetCharacteristicsAsync();

                            await c[0].WriteValueWithoutResponseAsync(Encoding.UTF8.GetBytes(sendCommand));

                            awoken = true;
                        }
                    }
                }
                catch (Exception e)
                {

                }

                if (!awoken)
                {
                    try
                    {
                        foreach (BluetoothDevice bd in await Bluetooth.ScanForDevicesAsync())
                        {
                            if (bd.Id == deviceId)
                            {
                                var serv = await bd.Gatt.GetPrimaryServiceAsync((BluetoothUuid)serviceUuid);

                                if (serv != null)
                                {
                                    Thread.Sleep(5000);

                                    var c = await serv.GetCharacteristicsAsync();

                                    await c[0].WriteValueWithoutResponseAsync(Encoding.UTF8.GetBytes(sendCommand));

                                    awoken = true;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {

                    }
                }
#endif
            }

            Console.WriteLine("Bluetooth Activation " + (awoken? "Successful." : "Unsuccessful.\n"));

            if (awoken && sleepOnCompletion > 0)
            {
                Console.Write("Pause after Bluetooth Activation");

                for (var i = 0; i < sleepOnCompletion; i++)
                {
                    Thread.Sleep(1000);
                    Console.Write(".");
                }

                Console.WriteLine(Environment.NewLine);
            }

            return awoken;
        }

        public async Task<bool?> BluetoothWakeUpAttemptsAsync(Camera camera, int numberOfAttempts)
        {
            if (camera.BluetoothActivate == false)
                return null;
            
            int attempts = 0;
            bool? success = false;

            do
            {
                success = await BluetoothWakeupAsync(camera);
                attempts++;

                if (!(success == null || success == true))
                {
                    Thread.Sleep(10 * 1000);
                }
            }
            while ((attempts < numberOfAttempts) && !(success == null || success == true));

            return success;
        }
    }
}