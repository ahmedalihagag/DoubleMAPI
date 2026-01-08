using BLL.DTOs.CourseDTOs;
using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DoubleMAPI.Controllers
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : ControllerBase
    {
        private readonly ICourseAccessCodeService _codeService;
        private readonly ILogger _logger;

        public AdminController(ICourseAccessCodeService codeService)
        {
            _codeService = codeService;
            _logger = Log.ForContext<AdminController>();
        }

        /// <summary>
        /// Generate bulk access codes for a course
        /// </summary>
        [HttpPost("courses/{courseId}/bulk-generate-codes")]
        public async Task<ActionResult<IEnumerable<CourseAccessCodeDto>>> BulkGenerateCodes(
            int courseId,
            [FromBody] GenerateBulkCodesRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid input" });

            try
            {
                var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(adminId))
                    return Unauthorized(new { success = false, message = "Admin ID not found" });

                var codes = await _codeService.BulkGenerateCodesAsync(courseId, adminId, dto.Quantity);
                return Ok(new { success = true, count = codes.Count(), data = codes });
            }
            catch (ArgumentException ex)
            {
                _logger.Warning(ex, "Invalid argument: {Message}", ex.Message);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error bulk generating codes");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Export codes to PDF (Admin only)
        /// </summary>
        [HttpGet("courses/{courseId}/export-codes")]
        public async Task<IActionResult> ExportCodesToPdf(int courseId)
        {
            try
            {
                // TODO: Implement PDF export using iTextSharp or similar
                _logger.Information("Exporting codes for course {CourseId} to PDF", courseId);

                return Ok(new { success = true, message = "PDF export will be available at download link" });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error exporting codes to PDF");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Disable an access code
        /// </summary>
        [HttpPost("codes/{code}/disable")]
        public async Task<IActionResult> DisableCode(string code)
        {
            try
            {
                var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(adminId))
                    return Unauthorized(new { success = false, message = "Admin ID not found" });

                var success = await _codeService.DisableCodeAsync(code, adminId);
                if (!success)
                    return NotFound(new { success = false, message = "Code not found or already disabled" });

                return Ok(new { success = true, message = "Code disabled successfully" });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error disabling code {Code}", code);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get code statistics
        /// </summary>
        [HttpGet("codes/{code}/stats")]
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
    }
}