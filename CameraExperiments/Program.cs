using CameraExperiments;
using SimpleWifi;
using System.ComponentModel;
using InTheHand.Bluetooth;
using System.Text;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

string fileName = "Settings.json";
string jsonString = File.ReadAllText(fileName);
var settings = JsonSerializer.Deserialize<CamerasList>(jsonString)!;

var cameras = settings.cameras;

//var str = JsonSerializer.Serialize(cameras);

foreach (var camera in settings.cameras)
{
    Console.WriteLine("Processing Camera: " + camera.FriendlyName + Environment.NewLine);
    
    var bluetoothService = new BluetoothService();
    var wifiService = new WifiService();

    var bluetoothAwoken = await bluetoothService.BluetoothWakeUpAttemptsAsync(camera, 3);

    if (bluetoothAwoken == null || bluetoothAwoken == true)
    {
        if (wifiService.WifiAvailable())
        {
            wifiService.WifiDisconnect();
            var success = wifiService.WifiConnectAttempts(camera, 3);
        }
    }
}