using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Enums;

namespace DAL.Entities
{
    public class DeviceSession
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string DeviceId { get; set; } = null!;

        [Required]
        public ClientType ClientType { get; set; } // 1=Web, 2=Android, 3=iOS

        [MaxLength(200)]
        public string DeviceInfo { get; set; } = string.Empty;

        [MaxLength(50)]
        public string IpAddress { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpiresAt { get; set; }

        public DateTime? DeactivatedAt { get; set; }

        // Enforce single device per user per client type
        public DateTime? ReplacedAt { get; set; }
        public int? ReplacedBySessionId { get; set; }
    }
}
