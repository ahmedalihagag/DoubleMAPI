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
    public class CourseProgressRepository : Repository<CourseProgress>, ICourseProgressRepository
    {
        public CourseProgressRepository(ApplicationDbContext context) : base(context) { }

        public async Task<CourseProgress?> GetProgressAsync(string studentId, int courseId)
        {
            try
            {
                return await _dbSet
                    .FirstOrDefaultAsync(cp => cp.StudentId == studentId && cp.CourseId == courseId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting course progress");
                throw;
            }
        }

        public async Task<IEnumerable<CourseProgress>> GetProgressByStudentAsync(string studentId)
        {
            try
            {
                return await _dbSet
                    .Where(cp => cp.StudentId == studentId)
                    .Include(cp => cp.Course)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting course progress for student: {StudentId}", studentId);
                throw;
            }
        }

        public async Task UpdateCompletionPercentageAsync(string studentId, int courseId)
        {
            try
            {
                var totalLessons = await _context.Lessons
                    .CountAsync(l => l.Section.CourseId == courseId && !l.IsDeleted);

                var completedLessons = await _context.LessonProgresses
                    .CountAsync(lp => lp.StudentId == studentId && lp.Lesson.Section.CourseId == courseId);

                var progress = await GetProgressAsync(studentId, courseId);

                if (progress != null && totalLessons > 0)
                {
                    progress.CompletionPercentage = (decimal)completedLessons / totalLessons * 100;
                    progress.LastAccessedAt = DateTime.UtcNow;
                    progress.UpdatedAt = DateTime.UtcNow;
                    _logger.Information("Updated course progress for student: {StudentId}, course: {CourseId}, completion: {Percentage}%",
                        studentId, courseId, progress.CompletionPercentage);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating completion percentage");
                throw;
            }
        }
    }
}
