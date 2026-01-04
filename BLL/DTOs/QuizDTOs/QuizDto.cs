using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.QuizDTOs
{
    public class QuizDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? TimeLimitMinutes { get; set; }
        public bool AllowReentry { get; set; }
        public int? AttemptsAllowed { get; set; }
        public decimal? PassingScore { get; set; }
        public bool ShowCorrectAnswers { get; set; }
    }
}
