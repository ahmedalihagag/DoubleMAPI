using BLL.DTOs.CourseDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    /// <summary>
    /// Service for managing one-time-use course access codes with 32-day expiration
    /// </summary>
    public interface ICourseAccessCodeService
    {
        /// <summary>
        /// Generate a new course access code (valid for 32 days, single-use)
        /// </summary>
        Task<CourseAccessCodeDto> GenerateCodeAsync(int courseId, string adminId);

        /// <summary>
        /// Get access code details by code
        /// </summary>
        Task<CourseAccessCodeDto?> GetByCodeAsync(string code);

        /// <summary>
        /// Get all active codes for a course
        /// </summary>
        Task<IEnumerable<CourseAccessCodeDto>> GetActiveCodesByCourseAsync(int courseId);

        /// <summary>
        /// Use/redeem a code for student enrollment
        /// </summary>
        Task<bool> UseCodeAsync(string code, string studentId, int courseId);

        /// <summary>
        /// Disable a code (admin action)
        /// </summary>
        Task<bool> DisableCodeAsync(string code, string disabledBy);

        /// <summary>
        /// Get usage statistics for a code
        /// </summary>
        Task<CourseAccessCodeStatsDto?> GetCodeStatsAsync(string code);

        /// <summary>
        /// Get all codes for a course with pagination
        /// </summary>
        Task<IEnumerable<CourseAccessCodeDto>> GetCourseCodesPagedAsync(int courseId, int pageNumber, int pageSize);

        /// <summary>
        /// Bulk generate codes
        /// </summary>
        Task<IEnumerable<CourseAccessCodeDto>> BulkGenerateCodesAsync(int courseId, string adminId, int quantity);
    }
}