using DAL.Entities;
using DAL.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IQuizService
    {
        Task<Quiz> CreateQuizAsync(Quiz quiz);
        Task<Quiz?> GetQuizWithQuestionsAsync(int quizId);
        Task<QuizAttempt> StartQuizAttemptAsync(int quizId, string studentId);
        Task<QuizAttempt> SubmitQuizAsync(int quizId, string studentId, Dictionary<int, int> answers);
        Task<PagedResult<QuizAttempt>> GetStudentAttemptsAsync(string studentId, PaginationParams paginationParams);
        Task<int> GetAttemptCountAsync(string studentId, int quizId);
    }
}
