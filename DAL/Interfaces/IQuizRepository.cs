using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IQuizRepository : IRepository<Quiz>
    {
        Task<Quiz?> GetQuizWithQuestionsAsync(int quizId);
        Task<IEnumerable<Quiz>> GetQuizzesByCourseAsync(int courseId);
    }
}
