namespace CameraExperiments
{
    internal class DataHelper
    {
        public static CameraDetails? GetCameraDetails(Camera? camera, List<CameraDetails>? cameraDetails)
        {
            var cameraDetail = cameraDetails?.Where(cd => cd.Type == camera?.Type).FirstOrDefault();

            if (!string.IsNullOrEmpty(camera?.HostNameOrIPOverride))
                cameraDetail!.HostNameOrIP = camera.HostNameOrIPOverride;

            return cameraDetail;
        }

        public static string IdToMacAddress(string id)
        {
            var output = "";

            for (var i = 0; i < id.Length; i++)
            {
                if ((i % 2 == 0) && (i != 0) )
                {
                    output += ":";
                }

                output += id.Substring(i, 1);
            }

            return output;
        }
    }
}
