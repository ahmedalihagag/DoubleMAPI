using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.LessonDTOs
{
    public class CreateLessonDto
    {
        [Required]
        [StringLength(200, MinimumLength = 3)]
        public string Title { get; set; } = string.Empty;

        [StringLength(5000)]
        public string? Description { get; set; }

        [Required]
        public int SectionId { get; set; }

        [Required]
        public int DisplayOrder { get; set; }

        [StringLength(1000)]
        public string? VideoUrl { get; set; }

        [StringLength(1000)]
        public string? MaterialUrl { get; set; }

        public int? DurationMinutes { get; set; }
    }
}
