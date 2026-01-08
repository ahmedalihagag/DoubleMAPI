using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.QuizDTOs
{
    public class CreateQuizDto
    {
        [Required]
        public int CourseId { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 3)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public int? TimeLimitMinutes { get; set; }

        [Range(0, 100)]
        public decimal? PassingScore { get; set; }

        public bool ShowCorrectAnswers { get; set; } = true;

        public bool ShuffleQuestions { get; set; } = false;

        public bool ShuffleOptions { get; set; } = false;
    }
}
