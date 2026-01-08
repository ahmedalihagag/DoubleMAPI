using BLL.DTOs.ParentStudentDTOs;
using BLL.Interfaces;
using BLL.Services;
using DAL.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DoubleMAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ParentOnly")]
    public class ParentController : ControllerBase
    {
        private readonly IParentService _parentService;
        private readonly IProgressService _progressService;
        private readonly IQuizService _quizService; // ✅ Add this
        private readonly Serilog.ILogger _logger;

        public ParentController(
            IParentService parentService,
            IProgressService progressService,
            IQuizService quizService) // ✅ Add this
        {
            _parentService = parentService;
            _progressService = progressService;
            _quizService = quizService; // ✅ Add this
            _logger = Log.ForContext<ParentController>();
        }

        /// <summary>
        /// Generate a parent link code for student (Parents only)
        /// </summary>
        [HttpPost("generate-link-code")]
        public async Task<ActionResult> GenerateLinkCode([FromQuery] string studentId)
        {
            try
            {
                var code = await _parentService.GenerateLinkCodeAsync(studentId);
                return Ok(new
                {
                    success = true,
                    message = "Link code generated successfully",
                    data = new { code, expiresIn = "24 hours" }
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error generating link code for student {StudentId}", studentId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Link parent to student using code (Parents only)
        /// </summary>
        [HttpPost("link-student")]
        public async Task<ActionResult> LinkStudent([FromBody] ParentLinkDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid input" });

            try
            {
                var parentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(parentId))
                    return Unauthorized(new { success = false, message = "Parent ID not found" });

                var success = await _parentService.LinkParentToStudentAsync(parentId, dto.LinkCode);
                if (!success)
                    return BadRequest(new { success = false, message = "Invalid or expired link code" });

                return Ok(new { success = true, message = "Student linked successfully" });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error linking student");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get linked students (Parents only)
        /// </summary>
        [HttpGet("linked-students")]
        public async Task<ActionResult<List<StudentInfoDto>>> GetLinkedStudents()
        {
            try
            {
                var parentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(parentId))
                    return Unauthorized(new { success = false, message = "Parent ID not found" });

                var students = await _parentService.GetLinkedStudentsAsync(parentId);
                return Ok(new { success = true, count = students.Count, data = students });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting linked students");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get student progress (Parents can see linked students' progress)
        /// </summary>
        [HttpGet("student/{studentId}/progress")]
        public async Task<ActionResult> GetStudentProgress(string studentId)
        {
            try
            {
                var parentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(parentId))
                    return Unauthorized(new { success = false, message = "Parent ID not found" });

                // ✅ Verify parent-student link
                var isLinked = await _parentService.IsLinkedAsync(parentId, studentId);
                if (!isLinked)
                {
                    return StatusCode(403, new { success = false, message = "Not linked to this student" }); // ✅ Fixed
                }

                // ✅ Get student progress
                var progress = await _progressService.GetStudentProgressAsync(studentId);

                return Ok(new { success = true, data = progress });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting student progress");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get student quiz attempts (Parents can see linked students' quiz results)
        /// </summary>
        [HttpGet("student/{studentId}/quiz-attempts")]
        public async Task<ActionResult> GetStudentQuizAttempts(
            string studentId,
            [FromQuery] PaginationParams paginationParams)
        {
            try
            {
                var parentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(parentId))
                    return Unauthorized(new { success = false, message = "Parent ID not found" });

                // ✅ Verify parent-student link
                var isLinked = await _parentService.IsLinkedAsync(parentId, studentId);
                if (!isLinked)
                {
                    return StatusCode(403, new { success = false, message = "Not linked to this student" }); // ✅ Fixed
                }

                // ✅ Get student quiz attempts
                var attempts = await _quizService.GetStudentAttemptsAsync(studentId, paginationParams);

                return Ok(new { success = true, data = attempts });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting student quiz attempts");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }
    }
}