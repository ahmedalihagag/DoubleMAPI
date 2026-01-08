using BLL.DTOs.EnrollmentDTOs;
using BLL.Interfaces;
using DAL.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Threading.Tasks;

namespace DoubleMAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EnrollmentController : ControllerBase
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly Serilog.ILogger _logger;

        public EnrollmentController(IEnrollmentService enrollmentService)
        {
            _enrollmentService = enrollmentService;
            _logger = Log.ForContext<EnrollmentController>();
        }

        /// <summary>
        /// Enroll student in course using access code
        /// </summary>
        [HttpPost("enroll-by-code")]
        [Authorize(Policy = "StudentOnly")]
        public async Task<IActionResult> EnrollByCode([FromBody] EnrollByCourseCodeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid input" });

            try
            {
                var studentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(studentId))
                    return Unauthorized(new { success = false, message = "Student ID not found" });

                var success = await _enrollmentService.EnrollStudentByCodeAsync(studentId, dto.CourseCode);
                if (!success)
                    return BadRequest(new { success = false, message = "Invalid or expired code" });

                return Ok(new { success = true, message = "Enrolled in course successfully" });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error enrolling student by code");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get student's enrollments (Students only)
        /// </summary>
        [HttpGet("my-enrollments")]
        [Authorize(Policy = "StudentOnly")]
        public async Task<IActionResult> GetMyEnrollments([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var studentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(studentId))
                    return Unauthorized(new { success = false, message = "Student ID not found" });

                var paginationParams = new PaginationParams { PageNumber = pageNumber, PageSize = pageSize };
                var enrollments = await _enrollmentService.GetStudentEnrollmentsAsync(studentId, paginationParams);

                return Ok(new { success = true, data = enrollments });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting student enrollments");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get course enrollments (Admin/Teacher only)
        /// </summary>
        [HttpGet("course/{courseId}")]
        [Authorize(Policy = "TeacherOrAdmin")]
        public async Task<IActionResult> GetCourseEnrollments(int courseId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var paginationParams = new PaginationParams { PageNumber = pageNumber, PageSize = pageSize };
                var enrollments = await _enrollmentService.GetCourseEnrollmentsAsync(courseId, paginationParams);

                return Ok(new { success = true, data = enrollments });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting course enrollments");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Unenroll student from course (Students only)
        /// </summary>
        [HttpPost("unenroll/{courseId}")]
        [Authorize(Policy = "StudentOnly")]
        public async Task<IActionResult> Unenroll(int courseId)
        {
            try
            {
                var studentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(studentId))
                    return Unauthorized(new { success = false, message = "Student ID not found" });

                var success = await _enrollmentService.UnenrollStudentAsync(studentId, courseId);
                if (!success)
                    return BadRequest(new { success = false, message = "Not enrolled in this course" });

                return Ok(new { success = true, message = "Unenrolled successfully" });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error unenrolling student");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }
    }
}