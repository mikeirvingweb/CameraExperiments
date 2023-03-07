using CameraExperiments;
using System.Text.Json;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Net;
using System.Threading;

var settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(Environment.CurrentDirectory + "//" + "Settings.json"))!;

var wifiService = new WifiService();

foreach (var camera in settings.cameras)
{
    if (camera.Enabled == null || camera.Enabled == true)
    {
        Console.WriteLine("Processing Camera: " + camera.FriendlyName + Environment.NewLine);

        var wifiSuccess = false;
        bool? bluetoothAwoken = null;

        if (wifiService.WifiAvailable())
        {
            wifiSuccess = wifiService.WifiIsConnectedTo(camera.WifiSSID);
        }

        if (!wifiSuccess) // skip activation if Wifi already connected
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
                    CameraDetails? cameraDetail = DataHelper.GetCameraDetails(camera, settings.cameraDetails);

                    var listPath = "http" + (cameraDetail?.Secure == true ? "s" : "") + "://" + cameraDetail?.HostNameOrIP + "/" + cameraDetail?.ListPath;

                    if(cameraDetail?.Type == CameraType.FlashAir)
                    {
                        listPath = listPath.Replace("$FOLDER$", (!string.IsNullOrEmpty(camera.RemoteFolder)) ? camera.RemoteFolder : "");
                    }

                    var httpClient = new HttpClient();

                    httpClient.Timeout = new TimeSpan(0, 5, 0);

                    var response = await httpClient.GetAsync(listPath);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var content = await response.Content.ReadAsStringAsync();

                        if (cameraDetail?.Type == CameraType.Ceyomur)
                        {
                            XDocument xDocument = XDocument.Parse(content);

                            var files = xDocument.XPathSelectElements("//File");

                            using (var client = new HttpClient())
                            {
                                foreach (var file in files)
                                {
                                    var fileName = file.XPathSelectElement("NAME")?.Value;
                                    var dateTime = Convert.ToDateTime(file.XPathSelectElement("TIME")?.Value);

                                    await FileActions.DownloadFile(client, camera, cameraDetail, "", fileName, dateTime, settings.localFilePath);
                                }
                            }
                        }
                        else if (cameraDetail?.Type == CameraType.FlashAir)
                        {
                            StringReader reader = new StringReader(content);

                            string? line = reader.ReadLine(); // header line

                            if (line != null)
                            {
                                using (var client = new HttpClient())
                                {
                                    while ((line = reader.ReadLine()) != null)
                                    {
                                        var lineVariables = line.Split(',');

                                        string path = lineVariables[0],
                                            file = lineVariables[1],
                                            type = lineVariables[3],
                                            date = lineVariables[4],
                                            time = lineVariables[5];

                                        if (type == "16" && cameraDetail.SubFolders == true) // folder
                                        {
                                            var httpClientSub = new HttpClient();

                                            httpClientSub.Timeout = new TimeSpan(0, 5, 0);

                                            var responseSub = await httpClientSub.GetAsync(listPath + "/" + file);

                                            if (responseSub.StatusCode == HttpStatusCode.OK)
                                            {
                                                var contentSub = await responseSub.Content.ReadAsStringAsync();

                                                StringReader readerSub = new StringReader(contentSub);

                                                string? lineSub = readerSub.ReadLine(); // header line

                                                if (lineSub != null)
                                                {
                                                    using (var clientSub = new HttpClient())
                                                    {
                                                        while ((lineSub = readerSub.ReadLine()) != null)
                                                        {
                                                            var lineVariablesSub = lineSub.Split(',');

                                                            string pathSub = lineVariablesSub[0],
                                                                fileSub = lineVariablesSub[1],
                                                                typeSub = lineVariablesSub[3],
                                                                dateSub = lineVariablesSub[4],
                                                                timeSub = lineVariablesSub[5];

                                                            if (typeSub == "32") // file (we will only delve one level deep)
                                                            {
                                                                if (camera.DeleteThumbnailFiles == true && !string.IsNullOrEmpty(camera.ThumbnailFileExtension))
                                                                {
                                                                    if(fileSub.EndsWith(camera.ThumbnailFileExtension))
                                                                    {
                                                                        await FileActions.DeleteFile(client, cameraDetail, camera.RemoteFolder + "/" + file, fileSub);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    await FileActions.DownloadFile(clientSub, camera, cameraDetail, camera.RemoteFolder + "/" + file, fileSub, DataHelper.DecimalsToDateTime(Convert.ToUInt16(dateSub), Convert.ToUInt16(timeSub)), settings.localFilePath);
                                                                }
                                                            }
                                                        }

                                                        // Delete Sub Folder
                                                        await FileActions.DeleteFile(clientSub, cameraDetail, camera.RemoteFolder, file);
                                                    }
                                                }
                                            }
                                        }
                                        else if (type == "32") // file
                                        {
                                            if (camera.DeleteThumbnailFiles == true && !string.IsNullOrEmpty(camera.ThumbnailFileExtension))
                                            {
                                                if (file.EndsWith(camera.ThumbnailFileExtension))
                                                {
                                                    await FileActions.DeleteFile(client, cameraDetail, camera.RemoteFolder, file);
                                                }
                                            }
                                            else
                                            {
                                                await FileActions.DownloadFile(client, camera, cameraDetail, camera.RemoteFolder, file, DataHelper.DecimalsToDateTime(Convert.ToUInt16(date), Convert.ToUInt16(time)), settings.localFilePath);
                                            }
                                        }
                                    };
                                }
                            }                            
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