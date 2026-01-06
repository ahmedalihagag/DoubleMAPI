using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class Section
    {
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }
        public virtual Course Course { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        public bool IsDeleted { get; set; } = false;

        public int DisplayOrder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    }
}
