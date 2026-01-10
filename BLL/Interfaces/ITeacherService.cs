namespace BLL.Interfaces;

using BLL.DTOs.CourseDTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Service for managing teacher courses and enrollment codes
/// </summary>
public interface ITeacherService
{
    /// <summary>
    /// Get all courses assigned to teacher
    /// </summary>
    Task<List<CourseDto>> GetTeacherCoursesAsync(string teacherId);

    /// <summary>
    /// Generate enrollment code for a course
    /// </summary>
    Task<CourseAccessCodeDto> GenerateEnrollmentCodeAsync(int courseId, string teacherId);

    /// <summary>
    /// Bulk generate enrollment codes
    /// </summary>
    Task<IEnumerable<CourseAccessCodeDto>> BulkGenerateCodesAsync(int courseId, string teacherId, int quantity);

    /// <summary>
    /// Get active codes for a course
    /// </summary>
    Task<IEnumerable<CourseAccessCodeDto>> GetActiveCodesAsync(int courseId, string teacherId);

    /// <summary>
    /// Disable an enrollment code
    /// </summary>
    Task<bool> DisableCodeAsync(string code, string teacherId);

    /// <summary>
    /// Get enrollment statistics for a course
    /// </summary>
    Task<CourseEnrollmentStatsDto> GetCourseStatsAsync(int courseId, string teacherId);
}
