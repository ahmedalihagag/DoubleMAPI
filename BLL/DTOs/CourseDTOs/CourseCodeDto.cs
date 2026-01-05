using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.CourseDTOs
{
    public class CourseCodeDto
    {
        public string Code { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string IssuedBy { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
        public DateTime? UsedAt { get; set; }
        public string? UsedBy { get; set; }
    }
}
