using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.DTOs.OptionDTOs;

namespace BLL.DTOs.QuestionDTOs
{
    public class CreateQuestionDto
    {
        public int QuizId { get; set; }
        public string Text { get; set; } = string.Empty;
        public string QuestionType { get; set; } = "MultipleChoice";
        public decimal Points { get; set; } = 1;
        public int DisplayOrder { get; set; }
        public string? DifficultyLevel { get; set; }
        public string? Explanation { get; set; }
        public List<CreateOptionDto> Options { get; set; } = new();
    }
}
