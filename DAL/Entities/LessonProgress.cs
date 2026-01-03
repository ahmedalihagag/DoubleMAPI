using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class LessonProgress
    {
        public int Id { get; set; }

        [Required]
        public string StudentId { get; set; }
        public virtual ApplicationUser Student { get; set; }

        [Required]
        public int LessonId { get; set; }
        public virtual Lesson Lesson { get; set; }

        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    }
}
