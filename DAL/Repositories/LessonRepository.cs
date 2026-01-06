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
    public class LessonRepository : Repository<Lesson>, ILessonRepository
    {
        private readonly ILogger _logger;

        public LessonRepository(ApplicationDbContext context) : base(context)
        {
            _logger = Log.ForContext<LessonRepository>();
        }

        #region CRUD with soft delete and logging

        public override async Task<Lesson?> GetByIdAsync(int id)
        {
            try
            {
                _logger.Debug("Getting Lesson by Id: {LessonId}", id);
                return await _dbSet.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting Lesson by Id: {LessonId}", id);
                throw;
            }
        }

        public override async Task<IEnumerable<Lesson>> GetAllAsync()
        {
            try
            {
                _logger.Debug("Getting all Lessons");
                return await _dbSet.AsNoTracking().Where(l => !l.IsDeleted).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting all Lessons");
                throw;
            }
        }

        public override async Task AddAsync(Lesson entity)
        {
            try
            {
                _logger.Debug("Adding Lesson: {Title}", entity.Title);
                await _dbSet.AddAsync(entity);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error adding Lesson: {Title}", entity.Title);
                throw;
            }
        }

        public override void Update(Lesson entity)
        {
            try
            {
                _logger.Debug("Updating Lesson: {LessonId}", entity.Id);
                entity.UpdatedAt = DateTime.UtcNow;
                _dbSet.Update(entity);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating Lesson: {LessonId}", entity.Id);
                throw;
            }
        }

        public override void Delete(Lesson entity)
        {
            try
            {
                _logger.Debug("Soft deleting Lesson: {LessonId}", entity.Id);
                entity.IsDeleted = true;
                entity.UpdatedAt = DateTime.UtcNow;
                _dbSet.Update(entity);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error soft deleting Lesson: {LessonId}", entity.Id);
                throw;
            }
        }

        public override void DeleteRange(IEnumerable<Lesson> entities)
        {
            try
            {
                _logger.Debug("Soft deleting {Count} Lessons", entities?.Count() ?? 0);
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedAt = DateTime.UtcNow;
                }
                _dbSet.UpdateRange(entities);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error soft deleting Lessons");
                throw;
            }
        }

        #endregion

        #region Lesson-specific methods

        public async Task<Lesson?> GetLessonWithSectionsAsync(int lessonId)
        {
            try
            {
                _logger.Debug("Getting Lesson with Sections by Id: {LessonId}", lessonId);
                return await _dbSet
                    .AsNoTracking()
                    .Include(l => l.Section)
                    .FirstOrDefaultAsync(l => l.Id == lessonId && !l.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting Lesson with Sections by Id: {LessonId}", lessonId);
                throw;
            }
        }

        public IQueryable<Lesson> GetLessonsByCourseId(int courseId)
        {
            try
            {
                _logger.Debug("Getting Lessons by CourseId: {CourseId}", courseId);
                return _dbSet
                    .AsNoTracking()
                    .Where(l => l.Section.CourseId == courseId && !l.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting Lessons by CourseId: {CourseId}", courseId);
                throw;
            }
        }

        public async Task<int> GetCompletedLessonsCountAsync(string studentId, int courseId)
        {
            try
            {
                _logger.Debug("Counting completed lessons for Student: {StudentId}, Course: {CourseId}", studentId, courseId);
                return await _context.LessonProgresses
                    .AsNoTracking()
                    .CountAsync(lp => lp.StudentId == studentId && lp.Lesson.Section.CourseId == courseId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error counting completed lessons for Student: {StudentId}, Course: {CourseId}", studentId, courseId);
                throw;
            }
        }

        public async Task<bool> IsLessonCompletedAsync(string studentId, int lessonId)
        {
            try
            {
                _logger.Debug("Checking if Lesson {LessonId} is completed by Student {StudentId}", lessonId, studentId);
                return await _context.LessonProgresses
                    .AsNoTracking()
                    .AnyAsync(lp => lp.StudentId == studentId && lp.LessonId == lessonId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error checking lesson completion for Student: {StudentId}, Lesson: {LessonId}", studentId, lessonId);
                throw;
            }
        }

        #endregion
    }
}
