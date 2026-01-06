using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace DAL.Entities
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [Required]
        public string TeacherId { get; set; }
        public virtual ApplicationUser Teacher { get; set; }

        [MaxLength(50)]
        public string? Category { get; set; }

        [MaxLength(50)]
        public string? Level { get; set; } // Beginner, Intermediate, Advanced

        public int? DurationHours { get; set; }

        public bool IsPublished { get; set; } = false;

        public DateTime? PublishedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }

        // Navigation Properties
        public virtual ICollection<Section> Sections { get; set; } = new List<Section>();
        public virtual ICollection<CourseEnrollment> Enrollments { get; set; } = new List<CourseEnrollment>();
        public virtual ICollection<CourseCode> CourseCodes { get; set; } = new List<CourseCode>();
        public virtual ICollection<CourseProgress> CourseProgresses { get; set; } = new List<CourseProgress>();
        public virtual ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();

        public ICollection<CourseAccessCode> AccessCodes { get; set; } = new List<CourseAccessCode>();
    }
}
