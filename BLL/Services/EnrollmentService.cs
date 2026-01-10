using AutoMapper;
using BLL.DTOs.EnrollmentDTOs;
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
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly ILogger _logger;

        public EnrollmentService(IUnitOfWork unitOfWork, IMapper mapper, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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
                    _logger.Warning("Student {StudentId} already enrolled in course {CourseId}", studentId, courseId);
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }

                // Get course details
                var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
                if (course == null)
                {
                    _logger.Warning("Course not found: {CourseId}", courseId);
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }

                // Create enrollment
                var enrollment = new CourseEnrollment
                {
                    StudentId = studentId,
                    CourseId = courseId,
                    EnrolledAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _unitOfWork.CourseEnrollments.AddAsync(enrollment);

                // Initialize course progress
                var courseProgress = new CourseProgress
                {
                    StudentId = studentId,
                    CourseId = courseId,
                    CompletionPercentage = 0,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.CourseProgresses.AddAsync(courseProgress);

                // Create notification
                var notification = new Notification
                {
                    UserId = studentId,
                    Title = "Course Enrollment",
                    Message = $"You've been enrolled in {course.Title}",
                    Type = "Enrollment",
                    Priority = "Medium",
                    ActionUrl = $"/courses/{courseId}",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Notifications.AddAsync(notification);

                // Commit transaction
                await _unitOfWork.CommitTransactionAsync();

                // Send email (outside transaction - non-critical)
                try
                {
                    //await _emailService.SendCourseEnrollmentEmailAsync(studentId, course.Title);
                }
                catch (Exception emailEx)
                {
                    _logger.Warning(emailEx, "Failed to send enrollment email to {StudentId}", studentId);
                }

                _logger.Information("Student {StudentId} enrolled successfully in course {CourseId}", studentId, courseId);
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.Error(ex, "Error enrolling student {StudentId} in course {CourseId}", studentId, courseId);
                throw;
            }
        }

        /// <summary>
        /// ✅ FIXED: Enroll student using CourseAccessCode (not CourseCode)
        /// </summary>
        public async Task<bool> EnrollStudentByCodeAsync(string studentId, string courseCode)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                _logger.Information("Enrolling student {StudentId} using code: {Code}", studentId, courseCode);

                // ✅ FIXED: Use CourseAccessCodes repository (correct one)
                var now = DateTime.UtcNow;
                var code = await _unitOfWork.CourseAccessCodes.FindAsync(c =>
                    c.Code == courseCode &&
                    !c.IsUsed &&
                    !c.IsDisabled &&
                    c.ExpiresAt > now);

                if (code == null)
                {
                    _logger.Warning("Invalid or expired course code: {Code}", courseCode);
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }

                // Enroll student
                var enrolled = await EnrollStudentAsync(studentId, code.CourseId);

                if (enrolled)
                {
                    // Mark code as used
                    code.IsUsed = true;
                    code.UsedAt = DateTime.UtcNow;
                    code.UsedBy = studentId;
                    _unitOfWork.CourseAccessCodes.Update(code);

                    await _unitOfWork.CommitTransactionAsync();
                    _logger.Information("Student {StudentId} enrolled successfully using code: {Code}", studentId, courseCode);
                }
                else
                {
                    await _unitOfWork.RollbackTransactionAsync();
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

        public async Task<PagedResult<EnrollmentDto>> GetStudentEnrollmentsAsync(
            string studentId,
            PaginationParams paginationParams)
        {
            try
            {
                _logger.Debug("Getting enrollments for student: {StudentId}", studentId);

                var pagedEnrollments = await _unitOfWork.CourseEnrollments
                    .GetEnrollmentsByStudentPagedAsync(studentId, paginationParams);

                var enrollmentDtos = new List<EnrollmentDto>();

                foreach (var enrollment in pagedEnrollments.Items)
                {
                    var progress = await _unitOfWork.CourseProgresses
                        .GetProgressAsync(studentId, enrollment.CourseId);

                    var dto = _mapper.Map<EnrollmentDto>(enrollment);
                    dto.CompletionPercentage = progress?.CompletionPercentage ?? 0;

                    enrollmentDtos.Add(dto);
                }

                return new PagedResult<EnrollmentDto>(enrollmentDtos, pagedEnrollments.Metadata);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting enrollments for student: {StudentId}", studentId);
                throw;
            }
        }

        public async Task<PagedResult<EnrollmentDto>> GetCourseEnrollmentsAsync(
            int courseId,
            PaginationParams paginationParams)
        {
            try
            {
                _logger.Debug("Getting enrollments for course: {CourseId}", courseId);

                var pagedEnrollments = await _unitOfWork.CourseEnrollments
                    .GetEnrollmentsByCoursePagedAsync(courseId, paginationParams);

                var dtos = _mapper.Map<IEnumerable<EnrollmentDto>>(pagedEnrollments.Items);

                return new PagedResult<EnrollmentDto>(dtos, pagedEnrollments.Metadata);
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
                _logger.Information("Unenrolling student {StudentId} from course {CourseId}", studentId, courseId);

                var enrollment = await _unitOfWork.CourseEnrollments.FindAsync(e =>
                    e.StudentId == studentId && e.CourseId == courseId && e.IsActive);

                if (enrollment == null)
                {
                    _logger.Warning("Enrollment not found for student {StudentId} in course {CourseId}", studentId, courseId);
                    return false;
                }

                enrollment.IsActive = false;
                _unitOfWork.CourseEnrollments.Update(enrollment);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information("Student {StudentId} unenrolled from course {CourseId}", studentId, courseId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error unenrolling student {StudentId} from course {CourseId}", studentId, courseId);
                throw;
            }
        }

        public async Task<bool> IsStudentEnrolledAsync(string studentId, int courseId)
        {
            try
            {
                return await _unitOfWork.CourseEnrollments.IsStudentEnrolledAsync(studentId, courseId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error checking enrollment status");
                throw;
            }
        }

        public async Task<int> GetEnrollmentCountAsync(int courseId)
        {
            try
            {
                return await _unitOfWork.CourseEnrollments.GetEnrollmentCountByCourseAsync(courseId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting enrollment count for course: {CourseId}", courseId);
                throw;
            }
        }
    }
}
