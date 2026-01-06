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
    public class ParentLinkCodeRepository : Repository<ParentLinkCode>, IParentLinkCodeRepository
    {
        public ParentLinkCodeRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<ParentLinkCode?> GetByCodeAsync(string code)
        {
            try
            {
                _logger.Debug("Getting ParentLinkCode by Code: {Code}", code);

                return await _dbSet
                    .AsNoTracking()
                    .FirstOrDefaultAsync(plc => plc.Code == code && !plc.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting ParentLinkCode by Code: {Code}", code);
                throw;
            }
        }

        public async Task<IEnumerable<ParentLinkCode>> GetByStudentIdAsync(string studentId)
        {
            try
            {
                _logger.Debug("Getting ParentLinkCodes for StudentId: {StudentId}", studentId);

                return await _dbSet
                    .AsNoTracking()
                    .Where(plc => plc.StudentId == studentId && !plc.IsDeleted)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting ParentLinkCodes for StudentId: {StudentId}", studentId);
                throw;
            }
        }

        // Override Delete to soft-delete
        public override void Delete(ParentLinkCode entity)
        {
            try
            {
                _logger.Debug("Soft-deleting ParentLinkCode: {Id}", entity.Code);
                entity.IsDeleted = true;
                entity.UpdatedAt = DateTime.UtcNow;
                _dbSet.Update(entity);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error soft-deleting ParentLinkCode: {Id}", entity.Code);
                throw;
            }
        }
    }
}
