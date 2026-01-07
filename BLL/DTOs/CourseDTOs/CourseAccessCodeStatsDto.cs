using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.CourseDTOs
{
    public class CourseAccessCodeStatsDto
    {
        public string Code { get; set; } = null!;

        public int CourseId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; }

        public bool IsDisabled { get; set; }

        public bool IsExpired { get; set; }

        public int DaysRemaining { get; set; }

        public string CreatedBy { get; set; } = null!;

        public string? UsedBy { get; set; }

        public DateTime? UsedAt { get; set; }

        public DateTime? DisabledAt { get; set; }

        public string Status { get; set; } = "Valid"; // Valid, Expired, Used, Disabled
    }
}