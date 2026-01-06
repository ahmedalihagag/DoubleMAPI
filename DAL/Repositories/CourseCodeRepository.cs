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
    public class CourseCodeRepository : Repository<CourseCode>, ICourseCodeRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public CourseCodeRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
            _logger = Log.ForContext<CourseCodeRepository>();
        }

        public async Task<CourseCode?> GetByCodeAsync(string code)
        {
            return await _context.CourseCodes
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Code == code && !c.IsDeleted);
        }

        public async Task<CourseCode> GenerateCodeAsync(int courseId, string issuedBy, DateTime expiresAt)
        {
            var newCode = new CourseCode
            {
                Code = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper(),
                CourseId = courseId,
                IssuedBy = issuedBy,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                IsActive = true
            };

            await _context.CourseCodes.AddAsync(newCode);
            await _context.SaveChangesAsync();

            _logger.Information("Generated new CourseCode {Code} for CourseId {CourseId}", newCode.Code, courseId);
            return newCode;
        }

        public async Task<bool> DisableCodeAsync(string code)
        {
            var entity = await _context.CourseCodes.FirstOrDefaultAsync(c => c.Code == code && !c.IsDeleted);
            if (entity == null) return false;

            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;

            _context.CourseCodes.Update(entity);
            await _context.SaveChangesAsync();

            _logger.Information("Disabled CourseCode {Code}", code);
            return true;
        }

        public async Task<bool> EnableCodeAsync(string code)
        {
            var entity = await _context.CourseCodes.FirstOrDefaultAsync(c => c.Code == code && !c.IsDeleted);
            if (entity == null) return false;

            entity.IsActive = true;
            entity.UpdatedAt = DateTime.UtcNow;

            _context.CourseCodes.Update(entity);
            await _context.SaveChangesAsync();

            _logger.Information("Enabled CourseCode {Code}", code);
            return true;
        }

        // NEW: Get all active codes for a course
        public async Task<IEnumerable<CourseCode>> GetActiveCodesByCourseIdAsync(int courseId)
        {
            return await _context.CourseCodes
                .AsNoTracking()
                .Where(c => c.CourseId == courseId && !c.IsDeleted && c.IsActive && c.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
        }
    }
}
