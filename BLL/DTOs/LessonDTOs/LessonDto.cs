using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.LessonDTOs
{
    public class LessonDto
    {
        public int Id { get; set; }

        public int SectionId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int DisplayOrder { get; set; }

        public string? VideoUrl { get; set; }

        public string? MaterialUrl { get; set; }

        public int? DurationMinutes { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public bool IsCompleted { get; set; } = false;
    }
}
