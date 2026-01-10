using System;
using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs.CourseDTOs
{
    public class CreateCourseDto
    {
        [Required]
        [StringLength(200, MinimumLength = 3)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [StringLength(100)]
        public string? Category { get; set; }

        [StringLength(50)]
        public string? Level { get; set; }

        [Range(1, 1000)]
        public int? DurationHours { get; set; }

        public bool IsPublished { get; set; } = false;

        [StringLength(100)]
        public string? TeacherId { get; set; }
    }
}
