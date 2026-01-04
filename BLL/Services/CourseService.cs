using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;
using DAL.Pagination;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class CourseService : ICourseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;

        public CourseService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _logger = Log.ForContext<CourseService>();
        }

        public async Task<Course> CreateCourseAsync(Course course)
        {
            try
            {
                _logger.Information("Creating course: {Title} by teacher: {TeacherId}",
                    course.Title, course.TeacherId);

                await _unitOfWork.Courses.AddAsync(course);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information("Course created successfully: {CourseId}", course.Id);
                return course;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating course: {Title}", course.Title);
                throw;
            }
        }

        public async Task<Course?> GetCourseByIdAsync(int courseId)
        {
            try
            {
                _logger.Debug("Getting course by Id: {CourseId}", courseId);
                return await _unitOfWork.Courses.GetByIdAsync(courseId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting course: {CourseId}", courseId);
                throw;
            }
        }

        public async Task<Course?> GetCourseWithDetailsAsync(int courseId)
        {
            try
            {
                _logger.Debug("Getting course with details: {CourseId}", courseId);
                return await _unitOfWork.Courses.GetCourseWithSectionsAndLessonsAsync(courseId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting course details: {CourseId}", courseId);
                throw;
            }
        }

        public async Task<PagedResult<Course>> GetPublishedCoursesAsync(PaginationParams paginationParams)
        {
            try
            {
                _logger.Information("Getting published courses - Page: {Page}", paginationParams.PageNumber);
                return await _unitOfWork.Courses.GetPublishedCoursesPagedAsync(paginationParams);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting published courses");
                throw;
            }
        }

        public async Task<PagedResult<Course>> GetCoursesByTeacherAsync(
            string teacherId,
            PaginationParams paginationParams)
        {
            try
            {
                _logger.Information("Getting courses for teacher: {TeacherId}", teacherId);
                return await _unitOfWork.Courses.GetCoursesByTeacherPagedAsync(teacherId, paginationParams);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting courses for teacher: {TeacherId}", teacherId);
                throw;
            }
        }

        public async Task<PagedResult<Course>> SearchCoursesAsync(
            string searchTerm,
            PaginationParams paginationParams)
        {
            try
            {
                _logger.Information("Searching courses with term: {SearchTerm}", searchTerm);
                return await _unitOfWork.Courses.SearchCoursesPagedAsync(searchTerm, paginationParams);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error searching courses");
                throw;
            }
        }

        public async Task<bool> UpdateCourseAsync(Course course)
        {
            try
            {
                _logger.Information("Updating course: {CourseId}", course.Id);

                _unitOfWork.Courses.Update(course);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information("Course updated successfully: {CourseId}", course.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating course: {CourseId}", course.Id);
                throw;
            }
        }

        public async Task<bool> DeleteCourseAsync(int courseId)
        {
            try
            {
                _logger.Information("Deleting course: {CourseId}", courseId);

                var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
                if (course == null)
                {
                    _logger.Warning("Course not found: {CourseId}", courseId);
                    return false;
                }

                course.IsDeleted = true;
                course.DeletedAt = DateTime.UtcNow;

                _unitOfWork.Courses.Update(course);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information("Course soft-deleted successfully: {CourseId}", courseId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error deleting course: {CourseId}", courseId);
                throw;
            }
        }

        public async Task<bool> PublishCourseAsync(int courseId)
        {
            try
            {
                _logger.Information("Publishing course: {CourseId}", courseId);

                var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
                if (course == null)
                {
                    _logger.Warning("Course not found: {CourseId}", courseId);
                    return false;
                }

                course.IsPublished = true;
                course.PublishedAt = DateTime.UtcNow;

                _unitOfWork.Courses.Update(course);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information("Course published successfully: {CourseId}", courseId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error publishing course: {CourseId}", courseId);
                throw;
            }
        }
    }
}
