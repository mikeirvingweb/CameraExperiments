using CameraExperiments;
using System.Text.Json;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Net;

var settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText("Settings.json"))!;

var dataHelper = new DataHelper();

var wifiService = new WifiService();

foreach (var camera in settings.cameras)
{
    Console.WriteLine("Processing Camera: " + camera.FriendlyName + Environment.NewLine);

    var wifiSuccess = false;
    bool? bluetoothAwoken = null;

    if (wifiService.WifiAvailable())
    {
        wifiSuccess = wifiService.WifiIsConnectedTo(camera.WifiSSID);
    }

    if(!wifiSuccess) // skip activation if Wifi already connected
    {
        var bluetoothService = new BluetoothService();

        bluetoothAwoken = await bluetoothService.BluetoothWakeUpAttemptsAsync(camera, 3);
    }    

    if (bluetoothAwoken == null || bluetoothAwoken == true)
    {
        if (wifiService.WifiAvailable())
        {
            if (!wifiSuccess)
            {
                wifiService.WifiDisconnect();
                wifiSuccess = wifiService.WifiConnectAttempts(camera, 3);
            }

            if (wifiSuccess)
            {
                CameraDetails? cameraDetail = dataHelper.GetCameraDetails(camera, settings.cameraDetails);

                var httpClient = new HttpClient();

                var response = await httpClient.GetAsync("http" + (cameraDetail?.Secure == true ? "s" : "") + "://" + cameraDetail?.HostNameOrIP + "/" + cameraDetail?.ListPath);
                
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    XDocument xDocument = XDocument.Parse(content);

                    var files = xDocument.XPathSelectElements("//File");

                    using (var client = new HttpClient())
                    {
                        foreach (var file in files)
                        {
                            var fileName = file.XPathSelectElement("NAME")?.Value;
                            var fileSize = Convert.ToInt64(file.XPathSelectElement("SIZE")?.Value);
                            var dateTime = Convert.ToDateTime(file.XPathSelectElement("TIME")?.Value);

                            var saveFileName = settings.localFilePath + "/" + camera.FriendlyName!.Replace(" ", "-") + "/automated/" + dateTime.ToString("yyyy-MM-dd-HH-mm-ss") + "_" + camera.FriendlyName?.Replace(" ", "-") + "_" + fileName?.Replace("_", "-");

                            var download = await client.GetAsync("http" + (cameraDetail?.Secure == true ? "s" : "") + "://" + cameraDetail?.HostNameOrIP + "/" + cameraDetail?.FetchPath?.Replace("$FILENAME$", fileName));

                            if (download.StatusCode == HttpStatusCode.OK)
                            {
                                using (var fs = new FileStream(saveFileName, FileMode.CreateNew))
                                {
                                    await download.Content.CopyToAsync(fs);
                                }

                                if (File.Exists(saveFileName))
                                {
                                    File.SetCreationTime(saveFileName, dateTime);
                                    File.SetLastWriteTime(saveFileName, dateTime);
                                    File.SetLastAccessTime(saveFileName, dateTime);
                                }

                                if(camera.DeleteFiles == true)
                                {
                                    var delete = await client.GetAsync("http" + (cameraDetail?.Secure == true ? "s" : "") + "://" + cameraDetail?.HostNameOrIP + "/" + cameraDetail?.DeletePath?.Replace("$FILENAME$", fileName));
                                    if (delete.StatusCode == HttpStatusCode.OK)
                                    {
                                        await delete.Content.ReadAsStringAsync();
                                    }
                                }                                
                            }
                            
                            Console.WriteLine(fileName);
                        }
                    }
                }
            }
        }
    }
}

if(!wifiService.WifiIsConnectedTo(settings.localWifiSSID))
{
    wifiService.WifiDisconnect();
    wifiService.WifiConnectAttempts(null, 3);
}