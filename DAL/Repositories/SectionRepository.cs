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
    public class SectionRepository : ISectionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public SectionRepository(ApplicationDbContext context)
        {
            _context = context;
            _logger = Log.ForContext<SectionRepository>();
        }

        public async Task<int> CreateAsync(int courseId, string title, int displayOrder)
        {
            var section = new Section
            {
                CourseId = courseId,
                Title = title,
                DisplayOrder = displayOrder,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Sections.AddAsync(section);
            await _context.SaveChangesAsync();

            _logger.Information("Created Section Id {Id}", section.Id);

            return section.Id;
        }

        public async Task<bool> UpdateAsync(int sectionId, string title, int displayOrder)
        {
            var section = await _context.Sections
                .FirstOrDefaultAsync(s => s.Id == sectionId && !s.IsDeleted);

            if (section == null) return false;

            section.Title = title;
            section.DisplayOrder = displayOrder;
            section.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.Information("Updated Section Id {Id}", sectionId);
            return true;
        }

        public async Task<bool> SoftDeleteAsync(int sectionId)
        {
            var section = await _context.Sections
                .FirstOrDefaultAsync(s => s.Id == sectionId && !s.IsDeleted);

            if (section == null) return false;

            section.IsDeleted = true;
            section.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.Information("Soft deleted Section Id {Id}", sectionId);
            return true;
        }

        public async Task<(int Id, int CourseId, string Title, int DisplayOrder)?> GetByIdAsync(int sectionId)
        {
            var section = await _context.Sections
                .AsNoTracking()
                .Where(s => s.Id == sectionId && !s.IsDeleted)
                .Select(s => new { s.Id, s.CourseId, s.Title, s.DisplayOrder })
                .FirstOrDefaultAsync();

            if (section == null) return null;
            return (section.Id, section.CourseId, section.Title, section.DisplayOrder);
        }

        public async Task<IEnumerable<(int Id, int CourseId, string Title, int DisplayOrder)>> GetAllByCourseIdAsync(int courseId)
        {
            return await _context.Sections
                .AsNoTracking()
                .Where(s => s.CourseId == courseId && !s.IsDeleted)
                .Select(s => new { s.Id, s.CourseId, s.Title, s.DisplayOrder })
                .ToListAsync()
                .ContinueWith(t => t.Result.Select(s => (s.Id, s.CourseId, s.Title, s.DisplayOrder)));
        }
    }
}
