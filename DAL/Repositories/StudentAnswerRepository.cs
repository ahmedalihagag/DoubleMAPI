using DAL.Data;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class StudentAnswerRepository : Repository<StudentAnswer>, IStudentAnswerRepository
    {
        public StudentAnswerRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<StudentAnswer>> GetByQuizAttemptAsync(int quizAttemptId)
        {
            try
            {
                _logger.Debug("Getting StudentAnswers for QuizAttemptId: {QuizAttemptId}", quizAttemptId);

                return await _dbSet
                    .AsNoTracking()
                    .Where(sa => sa.QuizAttemptId == quizAttemptId && !sa.IsDeleted)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting StudentAnswers for QuizAttemptId: {QuizAttemptId}", quizAttemptId);
                throw;
            }
        }

        // Override Delete to soft-delete
        public override void Delete(StudentAnswer entity)
        {
            try
            {
                _logger.Debug("Soft-deleting StudentAnswer: {StudentAnswerId}", entity.Id);
                entity.IsDeleted = true;
                entity.UpdatedAt = DateTime.UtcNow;
                _dbSet.Update(entity);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error soft-deleting StudentAnswer: {StudentAnswerId}", entity.Id);
                throw;
            }
        }
    }
}
