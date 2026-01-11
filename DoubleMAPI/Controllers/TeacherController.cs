using BLL.DTOs.CourseDTOs;
using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DoubleMAPI.Controllers;

/// <summary>
/// ✅ Teacher management endpoints for courses and enrollment codes
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "TeacherOnly")]
public class TeacherController : ControllerBase
{
    private readonly ITeacherService _teacherService;
    private readonly Serilog.ILogger _logger;

    public TeacherController(ITeacherService teacherService)
    {
        _teacherService = teacherService;
        _logger = Log.ForContext<TeacherController>();
    }

    /// <summary>
    /// Get all courses assigned to teacher
    /// </summary>
    [HttpGet("courses")]
    public async Task<ActionResult<List<CourseDto>>> GetMyCourses()
    {
        try
        {
            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(teacherId))
                return Unauthorized(new { success = false, message = "Teacher ID not found" });

            var courses = await _teacherService.GetTeacherCoursesAsync(teacherId);
            return Ok(new { success = true, count = courses.Count, data = courses });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error getting teacher courses");
            return StatusCode(500, new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Generate enrollment code for a course
    /// </summary>
    [HttpPost("courses/{courseId}/generate-code")]
    public async Task<ActionResult> GenerateEnrollmentCode(int courseId)
    {
        try
        {
            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(teacherId))
                return Unauthorized(new { success = false, message = "Teacher ID not found" });

            var code = await _teacherService.GenerateEnrollmentCodeAsync(courseId, teacherId);
            return Ok(new
            {
                success = true,
                message = "Enrollment code generated successfully",
                data = code
            });
        }
        catch (UnauthorizedAccessException)
        {
            _logger.Warning("Unauthorized access to course {CourseId}", courseId);
            return StatusCode(403, new { success = false, message = "You do not own this course" });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error generating enrollment code");
            return StatusCode(500, new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Bulk generate enrollment codes
    /// </summary>
    [HttpPost("courses/{courseId}/generate-codes")]
    public async Task<ActionResult> BulkGenerateCodes(int courseId, [FromQuery] int quantity = 10)
    {
        try
        {
            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(teacherId))
                return Unauthorized(new { success = false, message = "Teacher ID not found" });

            var codes = await _teacherService.BulkGenerateCodesAsync(courseId, teacherId, quantity);
            return Ok(new
            {
                success = true,
                message = $"Generated {quantity} enrollment codes",
                data = codes
            });
        }
        catch (UnauthorizedAccessException)
        {
            _logger.Warning("Unauthorized access to course {CourseId}", courseId);
            return StatusCode(403, new { success = false, message = "You do not own this course" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error bulk generating codes");
            return StatusCode(500, new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Get active codes for a course
    /// </summary>
    [HttpGet("courses/{courseId}/active-codes")]
    public async Task<ActionResult> GetActiveCodes(int courseId)
    {
        try
        {
            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(teacherId))
                return Unauthorized(new { success = false, message = "Teacher ID not found" });

            var codes = await _teacherService.GetActiveCodesAsync(courseId, teacherId);
            return Ok(new { success = true, count = codes.Count(), data = codes });
        }
        catch (UnauthorizedAccessException)
        {
            _logger.Warning("Unauthorized access to course {CourseId}", courseId);
            return StatusCode(403, new { success = false, message = "You do not own this course" });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error getting active codes");
            return StatusCode(500, new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Disable an enrollment code
    /// </summary>
    [HttpPost("codes/{code}/disable")]
    public async Task<ActionResult> DisableCode(string code)
    {
        try
        {
            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(teacherId))
                return Unauthorized(new { success = false, message = "Teacher ID not found" });

            var success = await _teacherService.DisableCodeAsync(code, teacherId);
            if (!success)
                return NotFound(new { success = false, message = "Code not found" });

            return Ok(new { success = true, message = "Code disabled successfully" });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error disabling code");
            return StatusCode(500, new { success = false, message = "An error occurred" });
        }
    }

    /// <summary>
    /// Get enrollment statistics for a course
    /// </summary>
    [HttpGet("courses/{courseId}/statistics")]
    public async Task<ActionResult> GetCourseStatistics(int courseId)
    {
        try
        {
            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(teacherId))
                return Unauthorized(new { success = false, message = "Teacher ID not found" });

            var stats = await _teacherService.GetCourseStatsAsync(courseId, teacherId);
            return Ok(new { success = true, data = stats });
        }
        catch (UnauthorizedAccessException)
        {
            _logger.Warning("Unauthorized access to course {CourseId}", courseId);
            return StatusCode(403, new { success = false, message = "You do not own this course" });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error getting course statistics");
            return StatusCode(500, new { success = false, message = "An error occurred" });
        }
    }
}
