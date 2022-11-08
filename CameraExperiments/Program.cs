using CameraExperiments;
using SimpleWifi;
using System.ComponentModel;
using InTheHand.Bluetooth;
using System.Text;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

//var config = new ConfigurationBuilder()
               //.AddJsonFile("Settings.json", false)
               //.Build();

//var jCameras = config.GetChildren().FirstOrDefault(c => c.Key == "cameras").GetChildren();

string fileName = "Settings.json";
string jsonString = File.ReadAllText(fileName);
var j = JsonSerializer.Deserialize<CamerasList>(jsonString)!;



//var str = JsonSerializer.Serialize(cameras);

foreach (var camera in j.cameras)
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