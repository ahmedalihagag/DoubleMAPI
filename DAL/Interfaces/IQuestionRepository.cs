using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IQuestionRepository : IRepository<Question>
    {
        // Question-specific queries
        Task<Question?> GetQuestionWithOptionsAsync(int questionId);
        IQueryable<Question> GetQuestionsByQuizId(int quizId);
        Task<int> CountQuestionsByQuizIdAsync(int quizId);
    }
}
