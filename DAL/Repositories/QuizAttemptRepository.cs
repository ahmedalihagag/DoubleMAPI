using DAL.Data;
using DAL.Entities;
using DAL.Interfaces;
using DAL.Pagination;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class QuizAttemptRepository : Repository<QuizAttempt>, IQuizAttemptRepository
    {
        public QuizAttemptRepository(ApplicationDbContext context) : base(context) { }

        public async Task<PagedResult<QuizAttempt>> GetAttemptsByStudentPagedAsync(
            string studentId,
            PaginationParams paginationParams)
        {
            try
            {
                _logger.Information("Getting quiz attempts for student: {StudentId}", studentId);
                return await GetPagedAsync(
                    paginationParams,
                    qa => qa.StudentId == studentId,
                    qa => qa.Quiz
                );
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting quiz attempts for student: {StudentId}", studentId);
                throw;
            }
        }

        public async Task<PagedResult<QuizAttempt>> GetAttemptsByQuizPagedAsync(
            int quizId,
            PaginationParams paginationParams)
        {
            try
            {
                _logger.Information("Getting quiz attempts for quiz: {QuizId}", quizId);
                return await GetPagedAsync(
                    paginationParams,
                    qa => qa.QuizId == quizId,
                    qa => qa.Student
                );
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting quiz attempts for quiz: {QuizId}", quizId);
                throw;
            }
        }

        public async Task<IEnumerable<QuizAttempt>> GetAttemptsByStudentAndQuizAsync(string studentId, int quizId)
        {
            try
            {
                return await _dbSet
                    .Where(qa => qa.StudentId == studentId && qa.QuizId == quizId)
                    .OrderByDescending(qa => qa.StartedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting quiz attempts");
                throw;
            }
        }

        public async Task<int> GetAttemptCountAsync(string studentId, int quizId)
        {
            try
            {
                return await _dbSet.CountAsync(qa => qa.StudentId == studentId && qa.QuizId == quizId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error counting quiz attempts");
                throw;
            }
        }

        public async Task<QuizAttempt?> GetLatestAttemptAsync(string studentId, int quizId)
        {
            try
            {
                return await _dbSet
                    .Where(qa => qa.StudentId == studentId && qa.QuizId == quizId)
                    .OrderByDescending(qa => qa.StartedAt)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting latest quiz attempt");
                throw;
            }
        }
    }
}
