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
    public class CourseEnrollmentRepository : Repository<CourseEnrollment>, ICourseEnrollmentRepository
    {
        public CourseEnrollmentRepository(ApplicationDbContext context) : base(context) { }

        public async Task<PagedResult<CourseEnrollment>> GetEnrollmentsByStudentPagedAsync(
            string studentId,
            PaginationParams paginationParams)
        {
            try
            {
                _logger.Information("Getting enrollments for student: {StudentId}", studentId);
                return await GetPagedAsync(
                    paginationParams,
                    e => e.StudentId == studentId && e.IsActive,
                    e => e.Course,
                    e => e.Course.Teacher
                );
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting enrollments for student: {StudentId}", studentId);
                throw;
            }
        }

        public async Task<PagedResult<CourseEnrollment>> GetEnrollmentsByCoursePagedAsync(
            int courseId,
            PaginationParams paginationParams)
        {
            try
            {
                _logger.Information("Getting enrollments for course: {CourseId}", courseId);
                return await GetPagedAsync(
                    paginationParams,
                    e => e.CourseId == courseId && e.IsActive,
                    e => e.Student
                );
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting enrollments for course: {CourseId}", courseId);
                throw;
            }
        }

        public async Task<bool> IsStudentEnrolledAsync(string studentId, int courseId)
        {
            try
            {
                return await _dbSet.AnyAsync(e =>
                    e.StudentId == studentId && e.CourseId == courseId && e.IsActive);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error checking enrollment for student: {StudentId}, course: {CourseId}",
                    studentId, courseId);
                throw;
            }
        }

        public async Task<int> GetEnrollmentCountByCourseAsync(int courseId)
        {
            try
            {
                return await _dbSet.CountAsync(e => e.CourseId == courseId && e.IsActive);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error counting enrollments for course: {CourseId}", courseId);
                throw;
            }
        }
        public async Task<IEnumerable<string>> GetEnrolledStudentIdsAsync(int courseId)
        {
            try
            {
                _logger.Information("Getting enrolled student IDs for course: {CourseId}", courseId);

                return await _dbSet
                    .AsNoTracking()
                    .Where(e => e.CourseId == courseId && e.IsActive)
                    .Select(e => e.StudentId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting enrolled student IDs for course: {CourseId}", courseId);
                throw;
            }
        }
    }
}
