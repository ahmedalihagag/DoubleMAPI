using DAL.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs
{
    public class DeviceSessionDto
    {
        public string UserId { get; set; } = null!;
        public string DeviceId { get; set; } = null!;
        public ClientType ClientType { get; set; }
        public string DeviceInfo { get; set; } = null!;
        public string IPAddress { get; set; } = null!;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LogoutAt { get; set; }
        public string? DeactivatedByAdminId { get; set; }
    }
}
