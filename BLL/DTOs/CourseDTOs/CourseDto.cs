using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.CourseDTOs
{
    public class CourseDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public string TeacherId { get; set; } = string.Empty;

        public string? TeacherName { get; set; }

        public string? Category { get; set; }

        public string? Level { get; set; }

        public int? DurationHours { get; set; }

        public bool IsPublished { get; set; }

        public DateTime? PublishedAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int SectionCount { get; set; }

        public int EnrollmentCount { get; set; }
    }
}
