using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class Blacklist
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; }

        [MaxLength(50)]
        public string BlockType { get; set; } = "Permanent"; // Permanent, Temporary

        public DateTime BlockedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string BlockedBy { get; set; }
        public virtual ApplicationUser BlockedByUser { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public DateTime? UnblockedAt { get; set; }

        public string? UnblockedBy { get; set; }
    }
}
