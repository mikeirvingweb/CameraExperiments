using System.Net;

namespace CameraExperiments
{
    internal class FileActions
    {
        async static public Task DownloadFile(HttpClient client, Camera camera, CameraDetails cameraDetail, string folder, string fileName, DateTime dateTime, string localFilePath)
        {
            var discard = false;

            if (camera.DiscardFootageBetweenCertainTimes != null && camera.DiscardFootageBetweenCertainTimes == true)
            {
                if (!string.IsNullOrEmpty(camera.DiscardFootageBetweenCertainTimesStart) && !string.IsNullOrEmpty(camera.DiscardFootageBetweenCertainTimesEnd))
                {
                    string fileTime = dateTime.ToString("HH:mm:ss");

                    if (Convert.ToDateTime(fileTime) > Convert.ToDateTime(camera.DiscardFootageBetweenCertainTimesStart) && Convert.ToDateTime(fileTime) < Convert.ToDateTime(camera.DiscardFootageBetweenCertainTimesEnd))
                    {
                        discard = true;
                    }
                }
            }

            if (!discard)
            {
                var saveFileName = localFilePath + "/" + camera.FriendlyName!.Replace(" ", "-") + "/automated/" + dateTime.ToString("yyyy-MM-dd-HH-mm-ss") + "_" + camera.FriendlyName?.Replace(" ", "-") + "_" + fileName?.Replace("_", "-");

                Console.WriteLine("File: " + fileName + " - Requested.");

                var download = await client.GetAsync("http" + (cameraDetail?.Secure == true ? "s" : "") + "://" + cameraDetail?.HostNameOrIP + "/" + cameraDetail?.FetchPath?.Replace("$FOLDER$", folder).Replace("$FILENAME$", fileName));

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

                    if (camera.DeleteFiles == true)
                    {
                        await DeleteFile(client, cameraDetail, folder, fileName);
                    }
                }

                Console.WriteLine("File: " + fileName + " - Downloaded.");
            }
            else
            {
                if (camera.DeleteFiles == true)
                {
                    await DeleteFile(client, cameraDetail, folder, fileName);

                    Console.WriteLine("File: " + fileName + " - Discarded.");
                }
            }
        }

        async static public Task DeleteFile(HttpClient client, CameraDetails cameraDetail, string folder, string fileName)
        {
            var delete = await client.GetAsync("http" + (cameraDetail?.Secure == true ? "s" : "") + "://" + cameraDetail?.HostNameOrIP + "/" + cameraDetail?.DeletePath?.Replace("$FOLDER$", folder).Replace("$FILENAME$", fileName));
            if (delete.StatusCode == HttpStatusCode.OK)
            {
                await delete.Content.ReadAsStringAsync();
            }
        }
    }
}
