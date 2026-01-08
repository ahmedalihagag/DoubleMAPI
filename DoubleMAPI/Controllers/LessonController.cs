using BLL.DTOs.LessonDTOs;
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
    [Route("api/sections/{sectionId}/[controller]")]
    [Authorize]
    public class LessonController : ControllerBase
    {
        private readonly ILessonService _lessonService;
        private readonly Serilog.ILogger _logger;

        public LessonController(ILessonService lessonService)
        {
            _lessonService = lessonService;
            _logger = Log.ForContext<LessonController>();
        }

        /// <summary>
        /// Create a new lesson (Admin/Teacher only)
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "TeacherOrAdmin")]
        public async Task<ActionResult<LessonDto>> CreateLesson(int sectionId, [FromBody] CreateLessonDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid input", errors = ModelState.Values });

            if (dto.SectionId != sectionId)
                return BadRequest(new { success = false, message = "Section ID mismatch" });

            try
            {
                var lesson = await _lessonService.CreateLessonAsync(dto);
                return CreatedAtAction(nameof(GetLessonById), new { sectionId, lessonId = lesson.Id },
                    new { success = true, data = lesson });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating lesson for section {SectionId}", sectionId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get lesson by ID
        /// </summary>
        [HttpGet("{lessonId}")]
        public async Task<ActionResult<LessonDto>> GetLessonById(int sectionId, int lessonId)
        {
            try
            {
                var lesson = await _lessonService.GetLessonByIdAsync(lessonId);
                if (lesson == null || lesson.SectionId != sectionId)
                    return NotFound(new { success = false, message = "Lesson not found" });

                return Ok(new { success = true, data = lesson });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting lesson {LessonId}", lessonId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get all lessons for a section
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LessonDto>>> GetLessonsBySection(int sectionId)
        {
            try
            {
                var lessons = await _lessonService.GetLessonsBySectionAsync(sectionId);
                return Ok(new { success = true, count = lessons.Count, data = lessons });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting lessons for section {SectionId}", sectionId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Update lesson (Admin/Teacher only)
        /// </summary>
        [HttpPut("{lessonId}")]
        [Authorize(Policy = "TeacherOrAdmin")]
        public async Task<IActionResult> UpdateLesson(int sectionId, int lessonId, [FromBody] CreateLessonDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid input" });

            try
            {
                var success = await _lessonService.UpdateLessonAsync(lessonId, dto);
                if (!success)
                    return NotFound(new { success = false, message = "Lesson not found" });

                return Ok(new { success = true, message = "Lesson updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating lesson {LessonId}", lessonId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Delete lesson (Admin only)
        /// </summary>
        [HttpDelete("{lessonId}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteLesson(int sectionId, int lessonId)
        {
            try
            {
                var success = await _lessonService.DeleteLessonAsync(lessonId);
                if (!success)
                    return NotFound(new { success = false, message = "Lesson not found" });

                return Ok(new { success = true, message = "Lesson deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error deleting lesson {LessonId}", lessonId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Mark lesson as completed (Students only)
        /// </summary>
        [HttpPost("{lessonId}/complete")]
        [Authorize(Policy = "StudentOnly")]
        public async Task<IActionResult> MarkLessonComplete(int sectionId, int lessonId)
        {
            try
            {
                var studentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(studentId))
                    return Unauthorized(new { success = false, message = "Student ID not found" });

                var success = await _lessonService.MarkLessonCompleteAsync(studentId, lessonId);
                if (!success)
                    return BadRequest(new { success = false, message = "Could not mark lesson as complete" });

                return Ok(new { success = true, message = "Lesson marked as complete" });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error marking lesson complete {LessonId}", lessonId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }
    }
}