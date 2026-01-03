using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class QuizAttempt
    {
        public int Id { get; set; }

        [Required]
        public int QuizId { get; set; }
        public virtual Quiz Quiz { get; set; }

        [Required]
        public string StudentId { get; set; }
        public virtual ApplicationUser Student { get; set; }

        public decimal Score { get; set; }

        public decimal MaxScore { get; set; }

        public decimal Percentage { get; set; }

        public bool IsPassed { get; set; }

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        public DateTime? SubmittedAt { get; set; }

        public int TimeSpentMinutes { get; set; }

        // Navigation Properties
        public virtual ICollection<StudentAnswer> StudentAnswers { get; set; } = new List<StudentAnswer>();
    }
}
