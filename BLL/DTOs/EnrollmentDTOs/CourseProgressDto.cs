using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.EnrollmentDTOs
{
    public class CourseProgressDto
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public decimal CompletionPercentage { get; set; }
        public int TotalLessons { get; set; }
        public int CompletedLessons { get; set; }
        public DateTime? LastAccessedAt { get; set; }
    }
}
