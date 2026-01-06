using DAL.Data;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class QuestionRepository : Repository<Question>, IQuestionRepository
    {
        private readonly ILogger _logger;

        public QuestionRepository(ApplicationDbContext context) : base(context)
        {
            _logger = Log.ForContext<QuestionRepository>();
        }

        #region CRUD with soft delete

        public override async Task<Question?> GetByIdAsync(int id)
        {
            try
            {
                _logger.Debug("Getting Question by Id: {QuestionId}", id);
                return await _dbSet.AsNoTracking()
                                   .FirstOrDefaultAsync(q => q.Id == id && !q.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting Question by Id: {QuestionId}", id);
                throw;
            }
        }

        public override async Task<IEnumerable<Question>> GetAllAsync()
        {
            try
            {
                _logger.Debug("Getting all Questions");
                return await _dbSet.AsNoTracking().Where(q => !q.IsDeleted).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting all Questions");
                throw;
            }
        }

        public override async Task AddAsync(Question entity)
        {
            try
            {
                _logger.Debug("Adding Question: {QuestionText}", entity.Text);
                await _dbSet.AddAsync(entity);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error adding Question: {QuestionText}", entity.Text);
                throw;
            }
        }

        public override void Update(Question entity)
        {
            try
            {
                _logger.Debug("Updating Question: {QuestionId}", entity.Id);
                entity.UpdatedAt = DateTime.UtcNow;
                _dbSet.Update(entity);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating Question: {QuestionId}", entity.Id);
                throw;
            }
        }

        public override void Delete(Question entity)
        {
            try
            {
                _logger.Debug("Soft deleting Question: {QuestionId}", entity.Id);
                entity.IsDeleted = true;
                entity.UpdatedAt = DateTime.UtcNow;
                _dbSet.Update(entity);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error soft deleting Question: {QuestionId}", entity.Id);
                throw;
            }
        }

        public override void DeleteRange(IEnumerable<Question> entities)
        {
            try
            {
                _logger.Debug("Soft deleting {Count} Questions", entities?.Count() ?? 0);
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedAt = DateTime.UtcNow;
                }
                _dbSet.UpdateRange(entities);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error soft deleting Questions");
                throw;
            }
        }

        #endregion

        #region Question-specific methods

        public async Task<Question?> GetQuestionWithOptionsAsync(int questionId)
        {
            try
            {
                _logger.Debug("Getting Question with Options by Id: {QuestionId}", questionId);
                return await _dbSet
                    .AsNoTracking()
                    .Include(q => q.Options)
                    .FirstOrDefaultAsync(q => q.Id == questionId && !q.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting Question with Options by Id: {QuestionId}", questionId);
                throw;
            }
        }

        public IQueryable<Question> GetQuestionsByQuizId(int quizId)
        {
            try
            {
                _logger.Debug("Getting Questions by QuizId: {QuizId}", quizId);
                return _dbSet
                    .AsNoTracking()
                    .Where(q => q.QuizId == quizId && !q.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting Questions by QuizId: {QuizId}", quizId);
                throw;
            }
        }

        public async Task<int> CountQuestionsByQuizIdAsync(int quizId)
        {
            try
            {
                _logger.Debug("Counting Questions by QuizId: {QuizId}", quizId);
                return await _dbSet.AsNoTracking()
                                   .CountAsync(q => q.QuizId == quizId && !q.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error counting Questions by QuizId: {QuizId}", quizId);
                throw;
            }
        }

        #endregion
    }
}
