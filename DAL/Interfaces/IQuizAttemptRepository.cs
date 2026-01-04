using DAL.Entities;
using DAL.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IQuizAttemptRepository : IRepository<QuizAttempt>
    {
        Task<PagedResult<QuizAttempt>> GetAttemptsByStudentPagedAsync(string studentId, PaginationParams paginationParams);
        Task<PagedResult<QuizAttempt>> GetAttemptsByQuizPagedAsync(int quizId, PaginationParams paginationParams);
        Task<IEnumerable<QuizAttempt>> GetAttemptsByStudentAndQuizAsync(string studentId, int quizId);
        Task<int> GetAttemptCountAsync(string studentId, int quizId);
        Task<QuizAttempt?> GetLatestAttemptAsync(string studentId, int quizId);
    }
}
