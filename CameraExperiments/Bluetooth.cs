using InTheHand.Bluetooth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Text.Core;

namespace CameraExperiments
{
    internal class BluetoothService
    {
        public async Task<bool?> BluetoothWakeupAsync(Camera camera)
        {
            bool awoken = false;
            string? deviceId;
            BluetoothUuid? serviceGuid;
            string? sendCommand;
            int sleepOnCompletion = 0;

            if (camera.Type == CameraType.Ceyomur)
            {
                deviceId = "CE3234383230";
                serviceGuid = Guid.Parse("0000ffe0-0000-1000-8000-00805f9b34fb");
                sendCommand = "GPIO3";
                sleepOnCompletion = 15;
            }
            else
            {
                return null;
            }

            Console.WriteLine("Attempting Bluetooth Activation.");

            try // awake by specific id, if it can be seen
            {
                var bd = await BluetoothDevice.FromIdAsync(deviceId);

                if(bd != null)
                {
                    var serv = await bd.Gatt.GetPrimaryServiceAsync((BluetoothUuid)serviceGuid);

                    var c = await serv.GetCharacteristicsAsync();

                    await c[0].WriteValueWithoutResponseAsync(Encoding.UTF8.GetBytes(sendCommand));

                    awoken = true;
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

                            var c = await serv.GetCharacteristicsAsync();

                            await c[0].WriteValueWithoutResponseAsync(Encoding.UTF8.GetBytes(sendCommand));

                            awoken = true;
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
