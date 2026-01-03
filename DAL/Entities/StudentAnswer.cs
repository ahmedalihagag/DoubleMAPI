using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
     public class StudentAnswer
     {
        public int Id { get; set; }

        [Required]
        public int QuizAttemptId { get; set; }
        public virtual QuizAttempt QuizAttempt { get; set; }

        [Required]
        public int QuestionId { get; set; }
        public virtual Question Question { get; set; }

        [Required]
        public int SelectedOptionId { get; set; }

        public bool IsCorrect { get; set; }

        public decimal PointsEarned { get; set; }
     }
}
