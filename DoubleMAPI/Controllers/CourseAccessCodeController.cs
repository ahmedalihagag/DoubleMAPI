using BLL.DTOs.CourseDTOs;
using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DoubleMAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CourseAccessCodesController : ControllerBase
    {
        private readonly ICourseAccessCodeService _codeService;
        private readonly Serilog.ILogger _logger;

        public CourseAccessCodesController(ICourseAccessCodeService codeService)
        {
            _codeService = codeService;
            _logger = Log.ForContext<CourseAccessCodesController>();
        }

        /// <summary>
        /// Generate a single course access code (32-day validity, single-use)
        /// </summary>
        [HttpPost("generate")]
        [Authorize(Policy = "TeacherOrAdmin")]
        public async Task<ActionResult<CourseAccessCodeDto>> GenerateCode(
            [FromQuery] int courseId,
            [FromQuery] string adminId)
        {
            try
            {
                var code = await _codeService.GenerateCodeAsync(courseId, adminId);
                return Ok(new { success = true, data = code });
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warning(ex, "Invalid operation: {Message}", ex.Message);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error generating access code");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Bulk generate multiple course access codes
        /// </summary>
        [HttpPost("bulk-generate")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<IEnumerable<CourseAccessCodeDto>>> BulkGenerateCodes(
            [FromQuery] int courseId,
            [FromQuery] string adminId,
            [FromQuery] int quantity)
        {
            try
            {
                var codes = await _codeService.BulkGenerateCodesAsync(courseId, adminId, quantity);
                return Ok(new { success = true, count = codes.Count(), data = codes });
            }
            catch (ArgumentException ex)
            {
                _logger.Warning(ex, "Invalid argument: {Message}", ex.Message);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warning(ex, "Invalid operation: {Message}", ex.Message);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error bulk generating codes");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get a specific code's details
        /// </summary>
        [HttpGet("{code}")]
        [Authorize(Policy = "TeacherOrAdmin")]
        public async Task<ActionResult<CourseAccessCodeDto>> GetCode(string code)
        {
            try
            {
                var codeDto = await _codeService.GetByCodeAsync(code);
                if (codeDto == null)
                    return NotFound(new { success = false, message = "Code not found" });

                return Ok(new { success = true, data = codeDto });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting code");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get all active codes for a course
        /// </summary>
        [HttpGet("course/{courseId}/active")]
        [Authorize(Policy = "TeacherOrAdmin")]
        public async Task<ActionResult<IEnumerable<CourseAccessCodeDto>>> GetActiveCodes(int courseId)
        {
            try
            {
                var codes = await _codeService.GetActiveCodesByCourseAsync(courseId);
                return Ok(new { success = true, count = codes.Count(), data = codes });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting active codes");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Redeem a code for student enrollment (32-day access)
        /// </summary>
        [HttpPost("redeem")]
        [Authorize(Policy = "StudentOnly")]
        public async Task<IActionResult> RedeemCode(
            [FromQuery] string code,
            [FromQuery] int courseId)
        {
            try
            {
                var studentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(studentId))
                    return Unauthorized(new { success = false, message = "Student ID not found in token" });

                var success = await _codeService.UseCodeAsync(code, studentId, courseId);

                if (!success)
                    return BadRequest(new
                    {
                        success = false,
                        message = "Code is invalid, expired, already used, or you're already enrolled"
                    });

                return Ok(new
                {
                    success = true,
                    message = $"Successfully enrolled in course for 32 days access",
                    data = new { courseId, accessExpiresAt = System.DateTime.UtcNow.AddDays(32) }
                });
            }
            catch (ArgumentException ex)
            {
                _logger.Warning(ex, "Invalid argument: {Message}", ex.Message);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error redeeming code");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get code usage statistics
        /// </summary>
        [HttpGet("{code}/stats")]
        [Authorize(Policy = "TeacherOrAdmin")]
        public async Task<ActionResult<CourseAccessCodeStatsDto>> GetCodeStats(string code)
        {
            try
            {
                var stats = await _codeService.GetCodeStatsAsync(code);
                if (stats == null)
                    return NotFound(new { success = false, message = "Code not found" });

                return Ok(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting code stats");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get paged codes for a course
        /// </summary>
        [HttpGet("course/{courseId}/paged")]
        [Authorize(Policy = "TeacherOrAdmin")]
        public async Task<ActionResult<IEnumerable<CourseAccessCodeDto>>> GetCourseCodesPaged(
            int courseId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var codes = await _codeService.GetCourseCodesPagedAsync(courseId, pageNumber, pageSize);
                return Ok(new { success = true, count = codes.Count(), page = pageNumber, pageSize, data = codes });
            }
            catch (ArgumentException ex)
            {
                _logger.Warning(ex, "Invalid argument: {Message}", ex.Message);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting paged codes");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Disable a code (admin only)
        /// </summary>
        [HttpPost("{code}/disable")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DisableCode(
            string code,
            [FromQuery] string disabledBy)
        {
            try
            {
                var success = await _codeService.DisableCodeAsync(code, disabledBy);
                if (!success)
                    return NotFound(new { success = false, message = "Code not found or already disabled" });

                return Ok(new { success = true, message = "Code disabled successfully" });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error disabling code");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }
    }
}