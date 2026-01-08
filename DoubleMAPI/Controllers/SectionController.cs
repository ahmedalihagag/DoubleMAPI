using BLL.DTOs.SectionDTOs;
using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Threading.Tasks;

namespace DoubleMAPI.Controllers
{
    [ApiController]
    [Route("api/courses/{courseId}/[controller]")]
    [Authorize]
    public class SectionController : ControllerBase
    {
        private readonly ISectionService _sectionService;
        private readonly Serilog.ILogger _logger;

        public SectionController(ISectionService sectionService)
        {
            _sectionService = sectionService;
            _logger = Log.ForContext<SectionController>();
        }

        /// <summary>
        /// Create a new section (Admin/Teacher only)
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "TeacherOrAdmin")]
        public async Task<ActionResult<SectionDto>> CreateSection([FromBody] CreateSectionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid input", errors = ModelState.Values });

            if (dto.CourseId != courseId)
                return BadRequest(new { success = false, message = "Course ID mismatch" });

            try
            {
                var sectionId = await _sectionService.CreateSectionAsync(dto);
                var section = new SectionDto
                {
                    Id = sectionId,
                    CourseId = dto.CourseId,
                    Title = dto.Title,
                    DisplayOrder = dto.DisplayOrder,
                    CreatedAt = DateTime.UtcNow
                };

                return CreatedAtAction(nameof(GetSectionById), new { dto.CourseId, sectionId },
                    new { success = true, data = section });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating section for course {CourseId}", dto.CourseId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get section by ID
        /// </summary>
        [HttpGet("{sectionId}")]
        public async Task<ActionResult<SectionDto>> GetSectionById(int courseId, int sectionId)
        {
            try
            {
                var section = await _sectionService.GetSectionByIdAsync(sectionId);
                if (section == null || section.CourseId != courseId)
                    return NotFound(new { success = false, message = "Section not found" });

                return Ok(new { success = true, data = section });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting section {SectionId}", sectionId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get all sections for a course
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetSectionsByCourse(int courseId)
        {
            try
            {
                var sections = await _sectionService.GetSectionsByCourseAsync(courseId);
                return Ok(new { success = true, count = sections.Count, data = sections });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting sections for course {CourseId}", courseId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Update section (Admin/Teacher only)
        /// </summary>
        [HttpPut("{sectionId}")]
        [Authorize(Policy = "TeacherOrAdmin")]
        public async Task<IActionResult> UpdateSection(int courseId, int sectionId, [FromBody] CreateSectionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid input" });

            try
            {
                var success = await _sectionService.UpdateSectionAsync(sectionId, dto.Title, dto.DisplayOrder);
                if (!success)
                    return NotFound(new { success = false, message = "Section not found" });

                return Ok(new { success = true, message = "Section updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating section {SectionId}", sectionId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Delete section (Admin only)
        /// </summary>
        [HttpDelete("{sectionId}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteSection(int courseId, int sectionId)
        {
            try
            {
                var success = await _sectionService.DeleteSectionAsync(sectionId);
                if (!success)
                    return NotFound(new { success = false, message = "Section not found" });

                return Ok(new { success = true, message = "Section deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error deleting section {SectionId}", sectionId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }
    }
}