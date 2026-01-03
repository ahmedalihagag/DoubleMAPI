using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class Lesson
    {
        public int Id { get; set; }

        [Required]
        public int SectionId { get; set; }
        public virtual Section Section { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(1000)]
        public string? VideoUrl { get; set; }

        [MaxLength(1000)]
        public string? MaterialUrl { get; set; }

        [MaxLength(5000)]
        public string? Description { get; set; }

        public int DisplayOrder { get; set; }

        public int? DurationMinutes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; } = false;

        // Navigation Properties
        public virtual ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();
    }
}
