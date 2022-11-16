using CameraExperiments;
using System.Text.Json;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Net;

var settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText("Settings.json"))!;

var cameras = settings.cameras;
var cameraDetails = settings.cameraDetails;

var dataHelper = new DataHelper();

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

            if(success)
            {
                CameraDetails? cameraDetail = dataHelper.GetCameraDetails(camera, cameraDetails);

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

                            var saveFileName = @"c:\camera\" + dateTime.ToString("yyyy-MM-dd-hh-mm-ss") + "_" + camera.FriendlyName?.Replace(" ", "-") + "_" + fileName?.Replace("_", "-");

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