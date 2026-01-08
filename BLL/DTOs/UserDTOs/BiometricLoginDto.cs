using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.UserDTOs
{
    public class BiometricLoginDto
    {
        [Required]
        public string BiometricToken { get; set; } = string.Empty;

        [Required]
        public string DeviceId { get; set; } = string.Empty;

        [Required]
        public Enum ClientType { get; set; } // 1=Web, 2=Android, 3=iOS

        public string DeviceInfo { get; set; } = string.Empty;

        public string IpAddress { get; set; } = string.Empty;
    }
}