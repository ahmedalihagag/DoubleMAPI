using DAL.Entities;
using DAL.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class CourseCodeService : ICourseCodeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;

        public CourseCodeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _logger = Log.ForContext<CourseCodeService>();
        }

        public async Task<string> GenerateCourseCodeAsync(int courseId, string adminId, int expiryDays = 30)
        {
            try
            {
                _logger.Information("Admin {AdminId} generating course code for course: {CourseId}",
                    adminId, courseId);

                var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
                if (course == null)
                    throw new Exception("Course not found");

                var code = GenerateRandomCode();
                var expiresAt = DateTime.UtcNow.AddDays(expiryDays);

                var courseCode = new CourseCode
                {
                    Code = code,
                    CourseId = courseId,
                    IssuedBy = adminId,
                    ExpiresAt = expiresAt,
                    CreatedAt = DateTime.UtcNow,
                    IsUsed = false
                };

                await _unitOfWork.CourseCodes.AddAsync(courseCode);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information("Course code generated: {Code} by admin: {AdminId}", code, adminId);
                return code;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error generating course code");
                throw;
            }
        }

        public async Task<List<CourseCodeDto>> GetCourseCodesAsync(int courseId)
        {
            try
            {
                _logger.Debug("Getting course codes for course: {CourseId}", courseId);

                var codes = await _unitOfWork.CourseCodes
                    .FindAllAsync(c => c.CourseId == courseId);

                return codes.Select(c => new CourseCodeDto
                {
                    Code = c.Code,
                    CourseId = c.CourseId,
                    IssuedBy = c.IssuedBy,
                    ExpiresAt = c.ExpiresAt,
                    IsUsed = c.IsUsed,
                    UsedAt = c.UsedAt,
                    UsedBy = c.UsedBy
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting course codes");
                throw;
            }
        }

        public async Task<bool> ValidateCourseCodeAsync(string code)
        {
            try
            {
                var courseCode = await _unitOfWork.CourseCodes
                    .FindAsync(c => c.Code == code && !c.IsUsed && c.ExpiresAt > DateTime.UtcNow);

                return courseCode != null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error validating course code");
                throw;
            }
        }

        public async Task<bool> RevokeCourseCodeAsync(string code, string adminId)
        {
            try
            {
                _logger.Information("Admin {AdminId} revoking course code: {Code}", adminId, code);

                var courseCode = await _unitOfWork.CourseCodes.FindAsync(c => c.Code == code);
                if (courseCode == null)
                    return false;

                _unitOfWork.CourseCodes.Delete(courseCode);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information("Course code revoked: {Code}", code);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error revoking course code");
                throw;
            }
        }

        private string GenerateRandomCode()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper();
        }
    }

}
