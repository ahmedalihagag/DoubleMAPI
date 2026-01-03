using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace DAL.Entities
{
    public class ApplicationUser : IdentityUser
    {

        [MaxLength(100)]
        public string? FullName { get; set; }

        [MaxLength(500)]
        public string? ProfileImageUrl { get; set; }

        [MaxLength(5)]
        public string Language { get; set; } = "EN"; // EN or AR

        public bool DarkMode { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public virtual ICollection<Course> CoursesAsTeacher { get; set; } = new List<Course>();
        public virtual ICollection<CourseEnrollment> Enrollments { get; set; } = new List<CourseEnrollment>();
        public virtual ICollection<ParentStudent> ParentsLinked { get; set; } = new List<ParentStudent>();
        public virtual ICollection<ParentStudent> StudentsLinked { get; set; } = new List<ParentStudent>();
        public virtual ICollection<ParentLinkCode> LinkCodes { get; set; } = new List<ParentLinkCode>();
        public virtual ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();
        public virtual ICollection<CourseProgress> CourseProgresses { get; set; } = new List<CourseProgress>();
        public virtual ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public virtual ICollection<Blacklist> BlacklistEntries { get; set; } = new List<Blacklist>();
        public virtual ICollection<CourseCode> IssuedCourseCodes { get; set; } = new List<CourseCode>();
    }
}
