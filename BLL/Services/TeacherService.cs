namespace BLL.Services;

using AutoMapper;
using BLL.DTOs.CourseDTOs;
using BLL.Interfaces;
using DAL.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Service for managing teacher courses and enrollment codes
/// </summary>
public class TeacherService : ITeacherService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICourseAccessCodeService _codeService;
    private readonly ILogger _logger;

    public TeacherService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICourseAccessCodeService codeService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _codeService = codeService ?? throw new ArgumentNullException(nameof(codeService));
        _logger = Log.ForContext<TeacherService>();
    }

    /// <summary>
    /// Get all courses assigned to teacher
    /// </summary>
    public async Task<List<CourseDto>> GetTeacherCoursesAsync(string teacherId)
    {
        try
        {
            _logger.Information("Getting courses for teacher: {TeacherId}", teacherId);

            var courses = await _unitOfWork.Courses.FindAllAsync(c =>
                c.TeacherId == teacherId && !c.IsDeleted);

            var dtos = _mapper.Map<List<CourseDto>>(courses);

            _logger.Information("Retrieved {Count} courses for teacher: {TeacherId}", dtos.Count, teacherId);
            return dtos;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error getting teacher courses");
            throw;
        }
    }

    /// <summary>
    /// Generate enrollment code for a course
    /// </summary>
    public async Task<CourseAccessCodeDto> GenerateEnrollmentCodeAsync(int courseId, string teacherId)
    {
        try
        {
            // Verify teacher owns the course
            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            if (course == null || course.TeacherId != teacherId)
            {
                _logger.Warning("Teacher {TeacherId} unauthorized to generate code for course {CourseId}",
                    teacherId, courseId);
                throw new UnauthorizedAccessException("You do not own this course");
            }

            _logger.Information("Generating enrollment code for course {CourseId} by teacher {TeacherId}",
                courseId, teacherId);

            var code = await _codeService.GenerateCodeAsync(courseId, teacherId);

            _logger.Information("Generated enrollment code {Code} for course {CourseId}",
                code.Code, courseId);

            return code;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error generating enrollment code for course {CourseId}", courseId);
            throw;
        }
    }

    /// <summary>
    /// Bulk generate enrollment codes for a course
    /// </summary>
    public async Task<IEnumerable<CourseAccessCodeDto>> BulkGenerateCodesAsync(
        int courseId,
        string teacherId,
        int quantity)
    {
        try
        {
            if (quantity <= 0 || quantity > 1000)
                throw new ArgumentException("Quantity must be between 1 and 1000");

            // Verify teacher owns the course
            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            if (course == null || course.TeacherId != teacherId)
            {
                _logger.Warning("Teacher {TeacherId} unauthorized to generate codes for course {CourseId}",
                    teacherId, courseId);
                throw new UnauthorizedAccessException("You do not own this course");
            }

            _logger.Information("Bulk generating {Quantity} codes for course {CourseId} by teacher {TeacherId}",
                quantity, courseId, teacherId);

            var codes = await _codeService.BulkGenerateCodesAsync(courseId, teacherId, quantity);

            _logger.Information("Generated {Count} bulk enrollment codes for course {CourseId}",
                codes.Count(), courseId);

            return codes;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error bulk generating codes for course {CourseId}", courseId);
            throw;
        }
    }

    /// <summary>
    /// Get active codes for a course
    /// </summary>
    public async Task<IEnumerable<CourseAccessCodeDto>> GetActiveCodesAsync(int courseId, string teacherId)
    {
        try
        {
            // Verify teacher owns the course
            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            if (course == null || course.TeacherId != teacherId)
            {
                _logger.Warning("Teacher {TeacherId} unauthorized to view codes for course {CourseId}",
                    teacherId, courseId);
                throw new UnauthorizedAccessException("You do not own this course");
            }

            _logger.Debug("Getting active codes for course {CourseId}", courseId);

            var codes = await _codeService.GetActiveCodesByCourseAsync(courseId);

            _logger.Information("Retrieved {Count} active codes for course {CourseId}",
                codes.Count(), courseId);

            return codes;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error getting active codes for course {CourseId}", courseId);
            throw;
        }
    }

    /// <summary>
    /// Disable an enrollment code
    /// </summary>
    public async Task<bool> DisableCodeAsync(string code, string teacherId)
    {
        try
        {
            _logger.Information("Disabling code {Code} by teacher {TeacherId}", code, teacherId);

            var result = await _codeService.DisableCodeAsync(code, teacherId);

            if (result)
                _logger.Information("Code {Code} disabled successfully", code);
            else
                _logger.Warning("Code {Code} not found or already disabled", code);

            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error disabling code {Code}", code);
            throw;
        }
    }

    /// <summary>
    /// Get enrollment statistics for a course
    /// </summary>
    public async Task<CourseEnrollmentStatsDto> GetCourseStatsAsync(int courseId, string teacherId)
    {
        try
        {
            // Verify teacher owns the course
            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            if (course == null || course.TeacherId != teacherId)
            {
                _logger.Warning("Teacher {TeacherId} unauthorized to view stats for course {CourseId}",
                    teacherId, courseId);
                throw new UnauthorizedAccessException("You do not own this course");
            }

            _logger.Debug("Getting enrollment stats for course {CourseId}", courseId);

            var enrollmentCount = await _unitOfWork.CourseEnrollments
                .GetEnrollmentCountByCourseAsync(courseId);

            var activeCodes = await _codeService.GetActiveCodesByCourseAsync(courseId);

            var stats = new CourseEnrollmentStatsDto
            {
                CourseId = courseId,
                CourseName = course.Title,
                TotalEnrolled = enrollmentCount,
                ActiveCodesCount = activeCodes.Count(),
                UpdatedAt = DateTime.UtcNow
            };

            return stats;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error getting course stats");
            throw;
        }
    }
}
