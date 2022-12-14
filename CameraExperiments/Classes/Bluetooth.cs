using InTheHand.Bluetooth;
using System.Text;

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
            BluetoothUuid? serviceGuid;
            string? sendCommand;
            int sleepOnCompletion = 0;

            if (camera.Type == CameraType.Ceyomur)
            {
                deviceId = camera.BluetoothDeviceId;
                serviceGuid = Guid.Parse("0000ffe0-0000-1000-8000-00805f9b34fb");
                sendCommand = "GPIO3";
                sleepOnCompletion = 15;
            }
            else
            {
                return false;
            }

            Console.WriteLine("Attempting Bluetooth Activation.");

            try // awake by specific id, if it can be seen
            {
                var bd = await BluetoothDevice.FromIdAsync(deviceId);

                if(bd != null)
                {
                    var serv = await bd.Gatt.GetPrimaryServiceAsync((BluetoothUuid)serviceGuid);

                    if(serv != null)
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
                            var serv = await bd.Gatt.GetPrimaryServiceAsync((BluetoothUuid)serviceGuid);

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

            Console.WriteLine("Bluetooth Activation " + (awoken? "Successful." : "Unsuccessful."));

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
