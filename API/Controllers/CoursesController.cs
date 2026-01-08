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
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly ILogger _logger;

        public CoursesController(ICourseService courseService)
        {
            _courseService = courseService;
            _logger = Log.ForContext<CoursesController>();
        }

        /// <summary>
        /// Create a new course (Admin/Teacher only)
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "TeacherOrAdmin")]
        public async Task<ActionResult<CourseDto>> CreateCourse([FromBody] CreateCourseDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid input", errors = ModelState.Values });

            try
            {
                var courseId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(courseId))
                    return Unauthorized(new { success = false, message = "Teacher ID not found" });

                var course = await _courseService.CreateCourseAsync(dto);
                return CreatedAtAction(nameof(GetCourseById), new { courseId = course.Id },
                    new { success = true, data = course });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating course");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get all courses (paginated, available to all authenticated users)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseDto>>> GetAllCourses()
        {
            try
            {
                var courses = await _courseService.GetAllCoursesAsync();
                return Ok(new { success = true, count = courses.Count, data = courses });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting all courses");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get course by ID with sections and lessons
        /// </summary>
        [HttpGet("{courseId}")]
        public async Task<ActionResult<CourseDto>> GetCourseById(int courseId)
        {
            try
            {
                var course = await _courseService.GetCourseByIdAsync(courseId);
                if (course == null)
                    return NotFound(new { success = false, message = "Course not found" });

                return Ok(new { success = true, data = course });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting course {CourseId}", courseId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Update course (Admin only)
        /// </summary>
        [HttpPut("{courseId}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdateCourse(int courseId, [FromBody] CreateCourseDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid input" });

            try
            {
                var success = await _courseService.UpdateCourseAsync(courseId, dto);
                if (!success)
                    return NotFound(new { success = false, message = "Course not found" });

                return Ok(new { success = true, message = "Course updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating course {CourseId}", courseId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Delete course (soft delete, Admin only)
        /// </summary>
        [HttpDelete("{courseId}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteCourse(int courseId)
        {
            try
            {
                var success = await _courseService.DeleteCourseAsync(courseId);
                if (!success)
                    return NotFound(new { success = false, message = "Course not found" });

                return Ok(new { success = true, message = "Course deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error deleting course {CourseId}", courseId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get all teachers and their courses (Students can view all without enrollment)
        /// </summary>
        [HttpGet("teachers/all")]
        public async Task<ActionResult> GetAllTeachersWithCourses()
        {
            try
            {
                var courses = await _courseService.GetAllCoursesAsync();
                var teacherCourses = courses.GroupBy(c => c.TeacherId).Select(g => new
                {
                    teacherId = g.Key,
                    teacherName = g.First().TeacherName,
                    courses = g.ToList()
                });

                return Ok(new { success = true, data = teacherCourses });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting teachers with courses");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }
    }
}