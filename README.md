
# Camera Experiments in C#
📹 C# Functionality for connecting to various\* Wildlife / Security Cameras and downloading their recording files.

\*Can currently to **Ceyomur** cameras, including the **Ceyomur CY95 Solar 4K Trail Camera**, or any device housing a **Toshiba FlashAir** SD Card. 
 
➕ More camera makes and models will be added over time

## How to use

✏️ Modify *Settings.json* to suit your setup.

`localFilePath` - set this to a folder where files can be downloaded.

📹  *For Ceyomur cameras..*  
`WifiSSID` - set this to the Wifi SSID of your camera.  
`WifiPassword` - set this to the Wifi Password of your camera.  
`BluetoothDeviceId` - set this to the Bluetooth Device ID of your camera.  

☁️  *For Toshiba FlashAir devices..*  
`WifiSSID` - set this to the Wifi SSID of your Toshiba FlashAir card.  
`WifiPassword` - set this to the Wifi Password of your Toshiba FlashAir card.  
`RemoteFolder` - set this to the folder where your camera / device stores files.  

## 🪟 Windows  

Uses `SimpleWifi` and `InTheHand.BlueToothLE`.

To execute, simply run `CameraExperiments.exe`.

## 🐧 Linux (Experimental)

Uses scripts / system commands in place of SimpleWifi and InTheHand.BlueToothLE.

To run, you may need to start `NetworkManager` first.

`sudo systemctl start NetworkManager`

**Wifi** requires `nmcli`, **Bluetooth** requires `hcitool` and `gatttool`.

You will need to add execute permissions on the two script files, and the main executable.

`chmod +x Scripts/BluetoothLinux.sh`  
`chmod +x Scripts/WifiLinux.sh`  

`chmod +x CameraExperiments`  

Then to execute, run:  

`sudo ./CameraExperiments`  

### Contributions

🍴 Feel free to Fork / Branch / Modify, raise any Pull Requests for changes.

#### Further reading  

🦔 Built as part of [.NET, IoT and Hedgehogs!](https://www.mike-irving.co.uk/web-design-blog/?blogid=122)


#### Credits

🙏 Thanks to [Peter Foot](https://github.com/peterfoot) for help with Bluetooth connectivity, achieved using the [InTheHand.BluetoothLE NuGet Package](https://www.nuget.org/packages/InTheHand.BluetoothLE), part of [32feet.NET](https://github.com/inthehand/32feet).
