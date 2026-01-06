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
    public class OptionRepository : Repository<Option>, IOptionRepository
    {
        public OptionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Option>> GetByQuestionIdAsync(int questionId)
        {
            try
            {
                _logger.Debug("Getting options for QuestionId: {QuestionId}", questionId);
                return await _dbSet
                    .AsNoTracking()
                    .Where(o => o.QuestionId == questionId && !o.IsDeleted)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting options for QuestionId: {QuestionId}", questionId);
                throw;
            }
        }

        // Override Delete to soft delete
        public override void Delete(Option entity)
        {
            try
            {
                _logger.Debug("Soft-deleting Option: {OptionId}", entity.Id);
                entity.IsDeleted = true;
                entity.UpdatedAt = DateTime.UtcNow;
                _dbSet.Update(entity);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error soft-deleting Option: {OptionId}", entity.Id);
                throw;
            }
        }
    }
}
