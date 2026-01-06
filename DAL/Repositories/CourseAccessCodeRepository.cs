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
    public class CourseAccessCodeRepository : Repository<CourseAccessCode>, ICourseAccessCodeRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<CourseAccessCode> _dbSet;
        private readonly ILogger _logger;

        public CourseAccessCodeRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
            _dbSet = context.Set<CourseAccessCode>();
            _logger = Log.ForContext<CourseAccessCodeRepository>();
        }

        public IQueryable<CourseAccessCode> Query()
        {
            return _dbSet.AsNoTracking().Where(c => !c.IsDisabled);
        }

        public async Task<IEnumerable<CourseAccessCode>> GetByCourseIdAsync(int courseId)
        {
            try
            {
                _logger.Information("Fetching access codes for CourseId {CourseId}", courseId);
                return await _dbSet
                    .AsNoTracking()
                    .Where(c => c.CourseId == courseId && !c.IsDisabled)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error fetching access codes for CourseId {CourseId}", courseId);
                throw;
            }
        }

        public async Task DisableCodeAsync(string code, string disabledBy)
        {
            try
            {
                var entity = await _dbSet.FirstOrDefaultAsync(c => c.Code == code && !c.IsDisabled);
                if (entity == null)
                {
                    _logger.Warning("Attempted to disable non-existing or already disabled code: {Code}", code);
                    return;
                }

                entity.IsDisabled = true;
                entity.DisabledAt = DateTime.UtcNow;

                Update(entity);
                await _context.SaveChangesAsync();

                _logger.Information("Disabled access code {Code}", code);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error disabling access code {Code}", code);
                throw;
            }
        }

        // Override AddAsync to log
        public override async Task AddAsync(CourseAccessCode entity)
        {
            try
            {
                await base.AddAsync(entity);
                _logger.Information("Added new CourseAccessCode: {Code}", entity.Code);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error adding CourseAccessCode: {Code}", entity.Code);
                throw;
            }
        }

        // Override Update to log
        public override void Update(CourseAccessCode entity)
        {
            try
            {
                base.Update(entity);
                _logger.Information("Updated CourseAccessCode Id {Id}", entity.Id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating CourseAccessCode Id {Id}", entity.Id);
                throw;
            }
        }

        // Override Delete for soft delete (disable)
        public override void Delete(CourseAccessCode entity)
        {
            try
            {
                entity.IsDisabled = true;
                entity.DisabledAt = DateTime.UtcNow;
                base.Update(entity); // soft delete
                _logger.Information("Soft deleted (disabled) CourseAccessCode Id {Id}", entity.Id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error soft deleting CourseAccessCode Id {Id}", entity.Id);
                throw;
            }
        }
    }
}
