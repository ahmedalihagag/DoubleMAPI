using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.CourseDTOs
{
    public class CourseAccessCodeDto
    {
        public string Code { get; set; } = null!;

        public int CourseId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; }

        public DateTime? UsedAt { get; set; }

        public string? UsedBy { get; set; }

        public bool IsDisabled { get; set; }

        public DateTime? DisabledAt { get; set; }

        public string CreatedBy { get; set; } = null!;

        /// <summary>
        /// Indicates if code is still valid (not expired, not used, not disabled)
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Days remaining until expiration
        /// </summary>
        public int? DaysRemaining { get; set; }
    }
}