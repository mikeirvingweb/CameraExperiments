using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraExperiments
{
    internal class CameraDetails
    {
        public CameraType Type { get; set; }
        public string? HostNameOrIP { get; set; }
        public bool? Secure { get; set; }
        public string? ListPath { get; set; }
        public string? FetchPath { get; set; }
        public string? DeletePath { get; set; }
        public bool? SubFolders { get; set; }
    }
}
