using DAL.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;

        [Required]
        [MaxLength(500)]
        public string TokenHash { get; set; } = null!;

        [MaxLength(200)]
        public string? DeviceInfo { get; set; }

        public ClientType ClientType { get; set; }

        [MaxLength(50)]
        public string? IpAddress { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public DateTime? LastUsedAt { get; set; }

        public bool IsRevoked { get; set; } = false;
        public DateTime? RevokedAt { get; set; }

        // 🔐 Token rotation support
        public int? ReplacedByTokenId { get; set; }
        public RefreshToken? ReplacedByToken { get; set; }

        [NotMapped]
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    }

}
