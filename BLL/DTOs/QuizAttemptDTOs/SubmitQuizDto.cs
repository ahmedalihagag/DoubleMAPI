using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.QuizAttemptDTOs
{
    public class SubmitQuizDto
    {
        public int QuizId { get; set; }
        public Dictionary<int, int> Answers { get; set; } = new(); // QuestionId -> OptionId
    }
}
