using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.DTOs.QuestionDTOs;

namespace BLL.DTOs.QuizDTOs
{
    public class QuizDetailDto : QuizDto
    {
        public List<QuestionDto> Questions { get; set; } = new();
    }
}
