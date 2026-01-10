using DAL.Enums;
using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs.UserDTOs
{
    public class BiometricLoginDto
    {
        [Required]
        public string BiometricToken { get; set; } = string.Empty;

        [Required]
        public string DeviceId { get; set; } = string.Empty;

        [Required]
        public ClientType ClientType { get; set; }
    }
}
