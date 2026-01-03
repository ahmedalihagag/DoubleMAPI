using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class CourseProgress
    {
        public int Id { get; set; }

        [Required]
        public string StudentId { get; set; }
        public virtual ApplicationUser Student { get; set; }

        [Required]
        public int CourseId { get; set; }
        public virtual Course Course { get; set; }

        public decimal CompletionPercentage { get; set; } = 0;

        public DateTime? LastAccessedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
