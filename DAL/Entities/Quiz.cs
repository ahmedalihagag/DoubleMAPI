using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class Quiz
    {
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }
        public virtual Course Course { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public int? TimeLimitMinutes { get; set; }

        public bool AllowReentry { get; set; } = false;

        public int? AttemptsAllowed { get; set; } // -1 = unlimited

        public decimal? PassingScore { get; set; }

        public bool ShowCorrectAnswers { get; set; } = true;

        public bool ShuffleQuestions { get; set; } = false;

        public bool ShuffleOptions { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
        public virtual ICollection<QuizAttempt> Attempts { get; set; } = new List<QuizAttempt>();
    }
}
