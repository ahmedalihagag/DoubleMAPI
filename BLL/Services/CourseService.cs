using AutoMapper;
using BLL.DTOs.CourseDTOs;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class CourseService : ICourseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly ILogger _logger;

        private const string CACHE_KEY_ALL = "courses:all";
        private const string CACHE_KEY_ID = "courses:{0}";
        private const int CACHE_EXPIRY_MINUTES = 30;

        public CourseService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = Log.ForContext<CourseService>();
        }

        public async Task<CourseDto> CreateCourseAsync(CreateCourseDto dto)
        {
            try
            {
                _logger.Information("Creating course: {Title}", dto.Title);

                if (string.IsNullOrWhiteSpace(dto.Title))
                    throw new ArgumentException("Course title is required", nameof(dto.Title));

                var course = _mapper.Map<Course>(dto);
                course.CreatedAt = DateTime.UtcNow;

                await _unitOfWork.Courses.AddAsync(course);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information("Course created successfully: {CourseId}", course.Id);

                // ✅ FIXED: Invalidate cache properly
                await _cacheService.RemoveAsync(CACHE_KEY_ALL);

                return _mapper.Map<CourseDto>(course);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating course");
                throw;
            }
        }

        public async Task<CourseDto?> GetCourseByIdAsync(int courseId)
        {
            try
            {
                _logger.Debug("Getting course by ID: {CourseId}", courseId);

                // Check cache first
                var cacheKey = string.Format(CACHE_KEY_ID, courseId);
                var cached = await _cacheService.GetAsync<CourseDto>(cacheKey);

                if (cached != null)
                {
                    _logger.Debug("Cache hit for course: {CourseId}", courseId);
                    return cached;
                }

                // Get from database
                var course = await _unitOfWork.Courses
                    .Query()
                    .AsNoTracking()
                    .Include(c => c.Sections)
                    .ThenInclude(s => s.Lessons)
                    .FirstOrDefaultAsync(c => c.Id == courseId && !c.IsDeleted);

                if (course == null)
                {
                    _logger.Warning("Course not found: {CourseId}", courseId);
                    return null;
                }

                var dto = _mapper.Map<CourseDto>(course);

                // Cache the result
                await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(CACHE_EXPIRY_MINUTES));

                _logger.Debug("Course retrieved and cached: {CourseId}", courseId);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting course: {CourseId}", courseId);
                throw;
            }
        }

        public async Task<List<CourseDto>> GetAllCoursesAsync()
        {
            try
            {
                _logger.Debug("Getting all courses");

                // Check cache first
                var cached = await _cacheService.GetAsync<List<CourseDto>>(CACHE_KEY_ALL);

                if (cached != null)
                {
                    _logger.Debug("Cache hit for all courses");
                    return cached;
                }

                // Get from database
                var courses = await _unitOfWork.Courses
                    .Query()
                    .AsNoTracking()
                    .Where(c => !c.IsDeleted)
                    .Include(c => c.Sections)
                    .ToListAsync();

                var dtos = _mapper.Map<List<CourseDto>>(courses);

                // Cache the results
                await _cacheService.SetAsync(CACHE_KEY_ALL, dtos, TimeSpan.FromMinutes(CACHE_EXPIRY_MINUTES));

                _logger.Information("Retrieved {Count} courses and cached", dtos.Count);
                return dtos;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting all courses");
                throw;
            }
        }

        public async Task<bool> UpdateCourseAsync(int courseId, CreateCourseDto dto)
        {
            try
            {
                _logger.Information("Updating course: {CourseId}", courseId);

                var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
                if (course == null)
                {
                    _logger.Warning("Course not found for update: {CourseId}", courseId);
                    return false;
                }

                course.Title = dto.Title ?? course.Title;
                course.Description = dto.Description ?? course.Description;
                course.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Courses.Update(course);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information("Course updated successfully: {CourseId}", courseId);

                // ✅ FIXED: Invalidate both specific and all courses cache
                var cacheKey = string.Format(CACHE_KEY_ID, courseId);
                await _cacheService.RemoveAsync(cacheKey);
                await _cacheService.RemoveAsync(CACHE_KEY_ALL);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating course: {CourseId}", courseId);
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
                    _logger.Warning("Course not found for delete: {CourseId}", courseId);
                    return false;
                }

                course.IsDeleted = true;
                course.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Courses.Update(course);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information("Course deleted successfully: {CourseId}", courseId);

                // ✅ FIXED: Invalidate both specific and all courses cache
                var cacheKey = string.Format(CACHE_KEY_ID, courseId);
                await _cacheService.RemoveAsync(cacheKey);
                await _cacheService.RemoveAsync(CACHE_KEY_ALL);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error deleting course: {CourseId}", courseId);
                throw;
            }
        }

        public async Task<string> GenerateCourseAccessCodeAsync(int courseId, string adminId)
        {
            try
            {
                _logger.Information("Generating access code for CourseId {CourseId}", courseId);

                var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
                if (course == null)
                    throw new InvalidOperationException("Course not found");

                var code = Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper();

                var accessCode = new CourseAccessCode
                {
                    Code = code,
                    CourseId = courseId,
                    CreatedBy = adminId,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(32),
                    IsUsed = false,
                    IsDisabled = false
                };

                await _unitOfWork.CourseAccessCodes.AddAsync(accessCode);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information("Access code generated: {Code}", code);
                return code;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error generating access code for CourseId {CourseId}", courseId);
                throw;
            }
        }
    }
}
