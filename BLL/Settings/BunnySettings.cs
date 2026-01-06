using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Settings
{
    public class BunnySettings
    {
        public string StorageZoneName { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string StoragePassword { get; set; } = string.Empty;
        public string CdnUrl { get; set; } = string.Empty;
        public string StorageEndpoint { get; set; } = string.Empty;
    }
}
