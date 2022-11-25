using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraExperiments
{
    internal class Settings
    {
        public string? localFilePath { get; set; }
        public List<Camera>? cameras { get; set; }

        public List<CameraDetails>? cameraDetails { get; set; }
    }
}
