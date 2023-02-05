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

        public static DateTime DecimalsToDateTime(ushort decDate, ushort decTime)
        {
            int year = ((decDate >> 9) & 0x7F) + 1980, month = (decDate >> 5) & 0x0F, day = decDate & 0x1F;
            int hour = (decTime >> 11) & 0x1F, minute = (decTime >> 5) & 0x3F, second = (decTime & 0x1F) * 2;

            month = (month == 0) ? 1 : month;
            day = (day == 0) ? 1 : day;

            return new DateTime(year, month, day, hour, minute, second);
        }
    }
}
