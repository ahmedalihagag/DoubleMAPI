using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class Question
    {
        public int Id { get; set; }

        [Required]
        public int QuizId { get; set; }
        public virtual Quiz Quiz { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Text { get; set; }

        [MaxLength(50)]
        public string QuestionType { get; set; } = "MultipleChoice"; // MultipleChoice, MultipleSelect, TrueFalse

        public decimal Points { get; set; } = 1;

        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public int DisplayOrder { get; set; }

        [MaxLength(50)]
        public string? DifficultyLevel { get; set; } // Easy, Medium, Hard

        [MaxLength(2000)]
        public string? Explanation { get; set; }

        // Navigation Properties
        public virtual ICollection<Option> Options { get; set; } = new List<Option>();
        public virtual ICollection<StudentAnswer> StudentAnswers { get; set; } = new List<StudentAnswer>();
    }
}
