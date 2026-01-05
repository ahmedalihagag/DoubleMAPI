using BLL.DTOs.QuizAttemptDTOs;
using BLL.DTOs.QuizDTOs;
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
        Task<QuizDetailDto> CreateQuizAsync(CreateQuizDto createQuizDto);
        Task<QuizDetailDto?> GetQuizWithQuestionsAsync(int quizId);
        Task<QuizAttemptDto> StartQuizAttemptAsync(int quizId, string studentId);
        Task<QuizAttemptDto> SubmitQuizAsync(int quizId, string studentId, Dictionary<int, int> answers);
        Task<PagedResult<QuizAttemptDto>> GetStudentAttemptsAsync(string studentId, PaginationParams paginationParams);
        Task<int> GetAttemptCountAsync(string studentId, int quizId);
    }
}
