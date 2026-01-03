using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class EmailLog
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string To { get; set; }

        [Required]
        [MaxLength(300)]
        public string Subject { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Sent, Failed

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? SentAt { get; set; }

        [MaxLength(2000)]
        public string? Error { get; set; }

        public int RetryCount { get; set; } = 0;
    }
}
