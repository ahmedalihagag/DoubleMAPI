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
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ILogger _logger;

        public EnrollmentService(IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _logger = Log.ForContext<EnrollmentService>();
        }

        public async Task<bool> EnrollStudentAsync(string studentId, int courseId)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                _logger.Information("Enrolling student {StudentId} in course {CourseId}", studentId, courseId);

                // Check if already enrolled
                var isEnrolled = await _unitOfWork.CourseEnrollments
                    .IsStudentEnrolledAsync(studentId, courseId);

                if (isEnrolled)
                {
                    _logger.Warning("Student {StudentId} already enrolled in course {CourseId}",
                        studentId, courseId);
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }

                // Create enrollment
                var enrollment = new CourseEnrollment
                {
                    StudentId = studentId,
                    CourseId = courseId,
                    EnrolledAt = DateTime.UtcNow
                };

                await _unitOfWork.CourseEnrollments.AddAsync(enrollment);

                // Initialize course progress
                var courseProgress = new CourseProgress
                {
                    StudentId = studentId,
                    CourseId = courseId,
                    CompletionPercentage = 0
                };

                await _unitOfWork.CourseProgresses.AddAsync(courseProgress);

                // Create notification
                var notification = new Notification
                {
                    UserId = studentId,
                    Title = "Course Enrollment",
                    Message = "You've been enrolled in a new course!",
                    Type = "Enrollment",
                    Priority = "Medium"
                };

                await _unitOfWork.Notifications.AddAsync(notification);

                await _unitOfWork.CommitTransactionAsync();

                // Send email (outside transaction)
                var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
                if (course != null)
                {
                    try
                    {
                        await _emailService.SendCourseEnrollmentEmailAsync(studentId, course.Title);
                    }
                    catch (Exception emailEx)
                    {
                        _logger.Warning(emailEx, "Failed to send enrollment email to {StudentId}", studentId);
                    }
                }

                _logger.Information("Student {StudentId} enrolled successfully in course {CourseId}",
                    studentId, courseId);
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.Error(ex, "Error enrolling student {StudentId} in course {CourseId}",
                    studentId, courseId);
                throw;
            }
        }

        public async Task<bool> EnrollStudentByCodeAsync(string studentId, string courseCode)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                _logger.Information("Enrolling student {StudentId} using code: {Code}", studentId, courseCode);

                var code = await _unitOfWork.CourseCodes.FindAsync(c =>
                    c.Code == courseCode && !c.IsUsed && c.ExpiresAt > DateTime.UtcNow);

                if (code == null)
                {
                    _logger.Warning("Invalid or expired course code: {Code}", courseCode);
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }

                var enrolled = await EnrollStudentAsync(studentId, code.CourseId);

                if (enrolled)
                {
                    code.IsUsed = true;
                    code.UsedAt = DateTime.UtcNow;
                    code.UsedBy = studentId;
                    _unitOfWork.CourseCodes.Update(code);
                    await _unitOfWork.CommitTransactionAsync();
                }

                return enrolled;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.Error(ex, "Error enrolling student {StudentId} with code", studentId);
                throw;
            }
        }

        public async Task<PagedResult<CourseEnrollment>> GetStudentEnrollmentsAsync(
            string studentId,
            PaginationParams paginationParams)
        {
            try
            {
                _logger.Debug("Getting enrollments for student: {StudentId}", studentId);
                return await _unitOfWork.CourseEnrollments
                    .GetEnrollmentsByStudentPagedAsync(studentId, paginationParams);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting enrollments for student: {StudentId}", studentId);
                throw;
            }
        }

        public async Task<PagedResult<CourseEnrollment>> GetCourseEnrollmentsAsync(
            int courseId,
            PaginationParams paginationParams)
        {
            try
            {
                _logger.Debug("Getting enrollments for course: {CourseId}", courseId);
                return await _unitOfWork.CourseEnrollments
                    .GetEnrollmentsByCoursePagedAsync(courseId, paginationParams);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting enrollments for course: {CourseId}", courseId);
                throw;
            }
        }

        public async Task<bool> UnenrollStudentAsync(string studentId, int courseId)
        {
            try
            {
                _logger.Information("Unenrolling student {StudentId} from course {CourseId}",
                    studentId, courseId);

                var enrollment = await _unitOfWork.CourseEnrollments.FindAsync(e =>
                    e.StudentId == studentId && e.CourseId == courseId && e.IsActive);

                if (enrollment == null)
                {
                    _logger.Warning("Enrollment not found for student {StudentId} in course {CourseId}",
                        studentId, courseId);
                    return false;
                }

                enrollment.IsActive = false;
                _unitOfWork.CourseEnrollments.Update(enrollment);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information("Student {StudentId} unenrolled from course {CourseId}",
                    studentId, courseId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error unenrolling student {StudentId} from course {CourseId}",
                    studentId, courseId);
                throw;
            }
        }
    }
}
