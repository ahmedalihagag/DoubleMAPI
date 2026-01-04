using DAL.Data;
using DAL.Entities;
using DAL.Interfaces;
using DAL.Pagination;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class CourseRepository : Repository<Course>, ICourseRepository
    {
        public CourseRepository(ApplicationDbContext context) : base(context) { }

        public async Task<PagedResult<Course>> GetPublishedCoursesPagedAsync(PaginationParams paginationParams)
        {
            try
            {
                _logger.Information("Getting published courses - Page: {Page}", paginationParams.PageNumber);

                var query = _dbSet
                    .Where(c => c.IsPublished && !c.IsDeleted)
                    .Include(c => c.Teacher)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(paginationParams.SearchTerm))
                {
                    var searchTerm = paginationParams.SearchTerm.ToLower();
                    query = query.Where(c =>
                        c.Title.ToLower().Contains(searchTerm) ||
                        c.Description.ToLower().Contains(searchTerm));
                }

                query = paginationParams.SortBy?.ToLower() switch
                {
                    "title" => paginationParams.SortDescending
                        ? query.OrderByDescending(c => c.Title)
                        : query.OrderBy(c => c.Title),
                    "publishedat" => paginationParams.SortDescending
                        ? query.OrderByDescending(c => c.PublishedAt)
                        : query.OrderBy(c => c.PublishedAt),
                    _ => query.OrderByDescending(c => c.PublishedAt)
                };

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                    .Take(paginationParams.PageSize)
                    .ToListAsync();

                _logger.Information("Retrieved {Count} published courses", items.Count);
                return new PagedResult<Course>(items, new PaginationMetadata(
                    paginationParams.PageNumber, paginationParams.PageSize, totalCount));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting published courses");
                throw;
            }
        }

        public async Task<PagedResult<Course>> GetCoursesByTeacherPagedAsync(
            string teacherId,
            PaginationParams paginationParams)
        {
            try
            {
                _logger.Information("Getting courses by teacher: {TeacherId}", teacherId);
                return await GetPagedAsync(paginationParams, c => c.TeacherId == teacherId && !c.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting courses by teacher: {TeacherId}", teacherId);
                throw;
            }
        }

        public async Task<PagedResult<Course>> SearchCoursesPagedAsync(
            string searchTerm,
            PaginationParams paginationParams)
        {
            try
            {
                _logger.Information("Searching courses with term: {SearchTerm}", searchTerm);
                var lowerSearchTerm = searchTerm.ToLower();
                return await GetPagedAsync(
                    paginationParams,
                    c => c.IsPublished && !c.IsDeleted &&
                        (c.Title.ToLower().Contains(lowerSearchTerm) ||
                         c.Description.ToLower().Contains(lowerSearchTerm))
                );
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error searching courses");
                throw;
            }
        }

        public async Task<Course?> GetCourseWithSectionsAndLessonsAsync(int courseId)
        {
            try
            {
                _logger.Debug("Getting course {CourseId} with sections and lessons", courseId);
                return await _dbSet
                    .Include(c => c.Sections.OrderBy(s => s.DisplayOrder))
                        .ThenInclude(s => s.Lessons.OrderBy(l => l.DisplayOrder))
                    .Include(c => c.Teacher)
                    .FirstOrDefaultAsync(c => c.Id == courseId && !c.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting course with details: {CourseId}", courseId);
                throw;
            }
        }

        public async Task<IEnumerable<Course>> GetCoursesByCategoryAsync(string category)
        {
            try
            {
                _logger.Debug("Getting courses by category: {Category}", category);
                return await _dbSet
                    .Where(c => c.Category == category && c.IsPublished && !c.IsDeleted)
                    .Include(c => c.Teacher)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting courses by category: {Category}", category);
                throw;
            }
        }
    }
}
