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
    public class LessonProgressRepository : Repository<LessonProgress>, ILessonProgressRepository
    {
        public LessonProgressRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<LessonProgress>> GetProgressByStudentAsync(string studentId)
        {
            try
            {
                _logger.Debug("Getting lesson progress for student: {StudentId}", studentId);
                return await _dbSet
                    .Where(lp => lp.StudentId == studentId)
                    .Include(lp => lp.Lesson)
                        .ThenInclude(l => l.Section)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting lesson progress for student: {StudentId}", studentId);
                throw;
            }
        }

        public async Task<IEnumerable<LessonProgress>> GetProgressByCourseAsync(string studentId, int courseId)
        {
            try
            {
                _logger.Debug("Getting lesson progress for student: {StudentId}, course: {CourseId}", studentId, courseId);
                return await _dbSet
                    .Where(lp => lp.StudentId == studentId && lp.Lesson.Section.CourseId == courseId)
                    .Include(lp => lp.Lesson)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting lesson progress");
                throw;
            }
        }

        public async Task<bool> IsLessonCompletedAsync(string studentId, int lessonId)
        {
            try
            {
                return await _dbSet.AnyAsync(lp => lp.StudentId == studentId && lp.LessonId == lessonId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error checking lesson completion");
                throw;
            }
        }

        public async Task<int> GetCompletedLessonsCountAsync(string studentId, int courseId)
        {
            try
            {
                return await _dbSet
                    .CountAsync(lp => lp.StudentId == studentId && lp.Lesson.Section.CourseId == courseId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error counting completed lessons");
                throw;
            }
        }
    }
}
