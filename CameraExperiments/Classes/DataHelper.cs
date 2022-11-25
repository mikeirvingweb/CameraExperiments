using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraExperiments
{
    internal class DataHelper
    {
        public CameraDetails? GetCameraDetails(Camera? camera, List<CameraDetails>? cameraDetails)
        {
            var cameraDetail = cameraDetails?.Where(cd => cd.Type == camera?.Type).FirstOrDefault();

            if (!string.IsNullOrEmpty(camera?.HostNameOrIPOverride))
                cameraDetail!.HostNameOrIP = camera.HostNameOrIPOverride;

            return cameraDetail;
        }
    }
}
