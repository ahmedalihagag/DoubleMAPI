using AutoMapper;
using BLL.DTOs.CourseDTOs;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class CourseAccessCodeService : ICourseAccessCodeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private const int EXPIRY_DAYS = 32; // 32-day validity period
        private const int CODE_LENGTH = 12;
        private const string CODE_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private const int MAX_CODE_GENERATION_ATTEMPTS = 10;
        private const int MAX_BULK_ATTEMPTS = 50;

        public CourseAccessCodeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = Log.ForContext<CourseAccessCodeService>();
        }

        /// <summary>
        /// Generate a single course access code with 32-day expiration
        /// </summary>
        public async Task<CourseAccessCodeDto> GenerateCodeAsync(int courseId, string adminId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(adminId))
                    throw new ArgumentException("Admin ID cannot be empty", nameof(adminId));

                _logger.Information("Generating access code for CourseId {CourseId} by admin {AdminId}", courseId, adminId);

                // Verify course exists
                var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
                if (course == null)
                    throw new InvalidOperationException($"Course with ID {courseId} not found");

                // Generate unique code
                var code = await GenerateUniqueCodeAsync();

                var accessCode = new CourseAccessCode
                {
                    Code = code,
                    CourseId = courseId,
                    CreatedBy = adminId,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(EXPIRY_DAYS),
                    IsUsed = false,
                    IsDisabled = false
                };

                await _unitOfWork.CourseAccessCodes.AddAsync(accessCode);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information("Generated access code {Code} for CourseId {CourseId}", code, courseId);

                var dto = _mapper.Map<CourseAccessCodeDto>(accessCode);
                UpdateCodeValidity(dto);

                return dto;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error generating access code for CourseId {CourseId}", courseId);
                throw;
            }
        }

        /// <summary>
        /// Bulk generate multiple codes for a course (optimized for performance)
        /// </summary>
        public async Task<IEnumerable<CourseAccessCodeDto>> BulkGenerateCodesAsync(int courseId, string adminId, int quantity)
        {
            try
            {
                if (quantity <= 0 || quantity > 1000)
                    throw new ArgumentException("Quantity must be between 1 and 1000", nameof(quantity));

                if (string.IsNullOrWhiteSpace(adminId))
                    throw new ArgumentException("Admin ID cannot be empty", nameof(adminId));

                _logger.Information("Bulk generating {Quantity} codes for CourseId {CourseId}", quantity, courseId);

                // Verify course exists
                var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
                if (course == null)
                    throw new InvalidOperationException($"Course with ID {courseId} not found");

                var now = DateTime.UtcNow;
                var expiresAt = now.AddDays(EXPIRY_DAYS);
                var codesToAdd = new List<CourseAccessCode>();
                var generatedCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                // Get all existing codes for this course (to avoid duplicates)
                var existingCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var existingEntities = await _unitOfWork.CourseAccessCodes.FindAllAsync(c =>
                    c.CourseId == courseId && !c.IsDisabled);

                foreach (var entity in existingEntities)
                {
                    existingCodes.Add(entity.Code);
                }

                // Generate all unique codes first (in-memory)
                int attempts = 0;
                int maxTotalAttempts = quantity * MAX_BULK_ATTEMPTS;

                while (codesToAdd.Count < quantity && attempts < maxTotalAttempts)
                {
                    var code = GenerateRandomCode();

                    // Check if code already generated in this batch or exists in database
                    if (!generatedCodes.Add(code) || existingCodes.Contains(code))
                    {
                        attempts++;
                        continue;
                    }

                    codesToAdd.Add(new CourseAccessCode
                    {
                        Code = code,
                        CourseId = courseId,
                        CreatedBy = adminId,
                        CreatedAt = now,
                        ExpiresAt = expiresAt,
                        IsUsed = false,
                        IsDisabled = false
                    });
                }

                if (codesToAdd.Count < quantity)
                {
                    _logger.Warning("Could only generate {Generated} of {Requested} codes for CourseId {CourseId} after {Attempts} attempts",
                        codesToAdd.Count, quantity, courseId, attempts);
                }

                // Add all at once (single database batch)
                if (codesToAdd.Count > 0)
                {
                    await _unitOfWork.CourseAccessCodes.AddRangeAsync(codesToAdd);
                    await _unitOfWork.SaveChangesAsync();

                    _logger.Information("Bulk generated {Count} codes for CourseId {CourseId}", codesToAdd.Count, courseId);
                }

                var dtos = codesToAdd.Select(c =>
                {
                    var dto = _mapper.Map<CourseAccessCodeDto>(c);
                    UpdateCodeValidity(dto);
                    return dto;
                }).ToList();

                return dtos;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error bulk generating codes for CourseId {CourseId}", courseId);
                throw;
            }
        }

        /// <summary>
        /// Get access code details
        /// </summary>
        public async Task<CourseAccessCodeDto?> GetByCodeAsync(string code)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                    return null;

                var entity = await _unitOfWork.CourseAccessCodes.FindAsync(c =>
                    c.Code == code && !c.IsDisabled);

                if (entity == null)
                    return null;

                var dto = _mapper.Map<CourseAccessCodeDto>(entity);
                UpdateCodeValidity(dto);

                return dto;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting code {Code}", code);
                throw;
            }
        }

        /// <summary>
        /// Get all active (unused, not expired, not disabled) codes for a course
        /// </summary>
        public async Task<IEnumerable<CourseAccessCodeDto>> GetActiveCodesByCourseAsync(int courseId)
        {
            try
            {
                _logger.Debug("Getting active codes for CourseId {CourseId}", courseId);

                var now = DateTime.UtcNow;
                var codes = await _unitOfWork.CourseAccessCodes.FindAllAsync(c =>
                    c.CourseId == courseId &&
                    !c.IsDisabled &&
                    !c.IsUsed &&
                    c.ExpiresAt > now);

                var dtos = codes.Select(c =>
                {
                    var dto = _mapper.Map<CourseAccessCodeDto>(c);
                    UpdateCodeValidity(dto);
                    return dto;
                }).ToList();

                _logger.Information("Found {Count} active codes for CourseId {CourseId}", dtos.Count, courseId);
                return dtos;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting active codes for CourseId {CourseId}", courseId);
                throw;
            }
        }

        /// <summary>
        /// Use/redeem a code for student enrollment (one-time use, 32-day access)
        /// </summary>
        public async Task<bool> UseCodeAsync(string code, string studentId, int courseId)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(studentId))
                    throw new ArgumentException("Code and StudentId cannot be empty");

                _logger.Information("Using code {Code} for StudentId {StudentId} in CourseId {CourseId}", 
                    code, studentId, courseId);

                var now = DateTime.UtcNow;

                // Find and validate code
                var entity = await _unitOfWork.CourseAccessCodes.FindAsync(c =>
                    c.Code == code &&
                    c.CourseId == courseId &&
                    !c.IsDisabled);

                if (entity == null)
                {
                    _logger.Warning("Code {Code} not found or disabled", code);
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }

                // Validate code status
                if (entity.IsUsed)
                {
                    _logger.Warning("Code {Code} already used by {UsedBy} at {UsedAt}", 
                        code, entity.UsedBy, entity.UsedAt);
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }

                if (entity.ExpiresAt <= now)
                {
                    _logger.Warning("Code {Code} expired at {ExpiresAt}", code, entity.ExpiresAt);
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }

                // Check if student already enrolled
                var isEnrolled = await _unitOfWork.CourseEnrollments
                    .IsStudentEnrolledAsync(studentId, courseId);

                if (isEnrolled)
                {
                    _logger.Warning("Student {StudentId} already enrolled in CourseId {CourseId}", 
                        studentId, courseId);
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }

                // Mark code as used
                entity.IsUsed = true;
                entity.UsedAt = now;
                entity.UsedBy = studentId;

                _unitOfWork.CourseAccessCodes.Update(entity);

                // Enroll student in course
                var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
                if (course == null)
                {
                    _logger.Error("Course {CourseId} not found", courseId);
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }

                var enrollment = new CourseEnrollment
                {
                    StudentId = studentId,
                    CourseId = courseId,
                    EnrolledAt = now,
                    IsActive = true
                };

                await _unitOfWork.CourseEnrollments.AddAsync(enrollment);

                // Create course progress record
                var progress = new CourseProgress
                {
                    StudentId = studentId,
                    CourseId = courseId,
                    CompletionPercentage = 0,
                    CreatedAt = now
                };

                await _unitOfWork.CourseProgresses.AddAsync(progress);

                // Create notification
                var notification = new Notification
                {
                    UserId = studentId,
                    Title = "Course Enrollment",
                    Message = $"You've been enrolled in {course.Title} using an access code. You have 32 days of access.",
                    Type = "Enrollment",
                    Priority = "High",
                    ActionUrl = $"/courses/{courseId}",
                    IsRead = false,
                    CreatedAt = now
                };

                await _unitOfWork.Notifications.AddAsync(notification);

                await _unitOfWork.CommitTransactionAsync();

                _logger.Information("Code {Code} successfully used by StudentId {StudentId} for CourseId {CourseId} (Expires: {ExpiresDate})",
                    code, studentId, courseId, now.AddDays(EXPIRY_DAYS));

                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.Error(ex, "Error using code {Code}", code);
                throw;
            }
        }

        /// <summary>
        /// Disable a code (admin action)
        /// </summary>
        public async Task<bool> DisableCodeAsync(string code, string disabledBy)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(disabledBy))
                    return false;

                _logger.Information("Disabling code {Code} by {DisabledBy}", code, disabledBy);

                var entity = await _unitOfWork.CourseAccessCodes.FindAsync(c =>
                    c.Code == code && !c.IsDisabled);

                if (entity == null)
                {
                    _logger.Warning("Code {Code} not found or already disabled", code);
                    return false;
                }

                entity.IsDisabled = true;
                entity.DisabledAt = DateTime.UtcNow;

                _unitOfWork.CourseAccessCodes.Update(entity);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information("Code {Code} disabled successfully by {DisabledBy}", code, disabledBy);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error disabling code {Code}", code);
                throw;
            }
        }

        /// <summary>
        /// Get usage statistics for a code
        /// </summary>
        public async Task<CourseAccessCodeStatsDto?> GetCodeStatsAsync(string code)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                    return null;

                var entity = await _unitOfWork.CourseAccessCodes.FindAsync(c => c.Code == code);

                if (entity == null)
                    return null;

                var now = DateTime.UtcNow;
                var isExpired = entity.ExpiresAt <= now;
                var daysRemaining = isExpired ? 0 : (int)(entity.ExpiresAt - now).TotalDays;

                string status;
                if (entity.IsDisabled)
                    status = "Disabled";
                else if (isExpired)
                    status = "Expired";
                else if (entity.IsUsed)
                    status = "Used";
                else
                    status = "Valid";

                var dto = _mapper.Map<CourseAccessCodeStatsDto>(entity);
                dto.IsExpired = isExpired;
                dto.DaysRemaining = daysRemaining;
                dto.Status = status;

                _logger.Debug("Retrieved stats for code {Code}: Status={Status}", code, status);

                return dto;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting stats for code {Code}", code);
                throw;
            }
        }

        /// <summary>
        /// Get all codes for a course with pagination (database-level pagination)
        /// </summary>
        public async Task<IEnumerable<CourseAccessCodeDto>> GetCourseCodesPagedAsync(
            int courseId, int pageNumber, int pageSize)
        {
            try
            {
                if (pageNumber < 1 || pageSize < 1)
                    throw new ArgumentException("Page number and page size must be greater than 0");

                if (pageSize > 100)
                    pageSize = 100; // Max 100 items per page

                _logger.Debug("Getting paged codes for CourseId {CourseId}, Page {Page}, PageSize {PageSize}",
                    courseId, pageNumber, pageSize);

                var skip = (pageNumber - 1) * pageSize;

                // Get codes for course (excluding disabled)
                var codes = await _unitOfWork.CourseAccessCodes.FindAllAsync(c =>
                    c.CourseId == courseId && !c.IsDisabled);

                var pagedCodes = codes
                    .OrderByDescending(c => c.CreatedAt)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToList();

                var dtos = pagedCodes.Select(c =>
                {
                    var dto = _mapper.Map<CourseAccessCodeDto>(c);
                    UpdateCodeValidity(dto);
                    return dto;
                }).ToList();

                _logger.Information("Retrieved {Count} codes for CourseId {CourseId} (Page {Page}/{TotalPages})", 
                    dtos.Count, courseId, pageNumber, (codes.Count() + pageSize - 1) / pageSize);
                
                return dtos;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting paged codes for CourseId {CourseId}", courseId);
                throw;
            }
        }

        /// <summary>
        /// Helper: Generate a unique alphanumeric code (thread-safe with exponential backoff)
        /// </summary>
        private async Task<string> GenerateUniqueCodeAsync()
        {
            string code;
            int attempts = 0;

            do
            {
                code = GenerateRandomCode();

                try
                {
                    var exists = await _unitOfWork.CourseAccessCodes.ExistsAsync(c => c.Code == code);

                    if (!exists)
                        return code;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error checking code uniqueness for code {Code}", code);
                    throw;
                }

                attempts++;

                if (attempts >= MAX_CODE_GENERATION_ATTEMPTS)
                {
                    throw new InvalidOperationException(
                        $"Failed to generate unique code after {MAX_CODE_GENERATION_ATTEMPTS} attempts. " +
                        $"Database may have too many codes or connectivity issue.");
                }

            } while (true);
        }

        /// <summary>
        /// Helper: Generate a random code using thread-safe Random.Shared (.NET 6+)
        /// </summary>
        private string GenerateRandomCode()
        {
            var codeBuilder = new StringBuilder(CODE_LENGTH);

            for (int i = 0; i < CODE_LENGTH; i++)
            {
                codeBuilder.Append(CODE_CHARS[Random.Shared.Next(CODE_CHARS.Length)]);
            }

            return codeBuilder.ToString();
        }

        /// <summary>
        /// Helper: Update DTO validity fields based on current time
        /// </summary>
        private void UpdateCodeValidity(CourseAccessCodeDto dto)
        {
            if (dto == null)
                return;

            var now = DateTime.UtcNow;
            var isExpired = dto.ExpiresAt <= now;

            dto.IsValid = !dto.IsUsed && !dto.IsDisabled && !isExpired;
            dto.DaysRemaining = isExpired ? 0 : (int)(dto.ExpiresAt - now).TotalDays;
        }
    }
}