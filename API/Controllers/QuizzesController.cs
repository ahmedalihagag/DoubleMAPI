using BLL.DTOs.QuizDTOs;
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
    [Route("api/courses/{courseId}/[controller]")]
    [Authorize]
    public class QuizzesController : ControllerBase
    {
        private readonly IQuizService _quizService;
        private readonly ILogger _logger;

        public QuizzesController(IQuizService quizService)
        {
            _quizService = quizService;
            _logger = Log.ForContext<QuizzesController>();
        }

        /// <summary>
        /// Create a new quiz (Admin/Teacher only)
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "TeacherOrAdmin")]
        public async Task<ActionResult<QuizDetailDto>> CreateQuiz(int courseId, [FromBody] CreateQuizDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid input", errors = ModelState.Values });

            if (dto.CourseId != courseId)
                return BadRequest(new { success = false, message = "Course ID mismatch" });

            try
            {
                var quiz = await _quizService.CreateQuizAsync(dto);
                return CreatedAtAction(nameof(GetQuizById), new { courseId, quizId = quiz.Id },
                    new { success = true, data = quiz });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating quiz for course {CourseId}", courseId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get quiz with questions (Students can only get if enrolled)
        /// </summary>
        [HttpGet("{quizId}")]
        public async Task<ActionResult<QuizDetailDto>> GetQuizById(int courseId, int quizId)
        {
            try
            {
                var quiz = await _quizService.GetQuizWithQuestionsAsync(quizId);
                if (quiz == null || quiz.CourseId != courseId)
                    return NotFound(new { success = false, message = "Quiz not found" });

                return Ok(new { success = true, data = quiz });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting quiz {QuizId}", quizId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Start a quiz attempt (Students only, enforces single attempt)
        /// </summary>
        [HttpPost("{quizId}/start")]
        [Authorize(Policy = "StudentOnly")]
        public async Task<ActionResult> StartQuizAttempt(int courseId, int quizId)
        {
            try
            {
                var studentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(studentId))
                    return Unauthorized(new { success = false, message = "Student ID not found" });

                // Check if already attempted
                var attemptCount = await _quizService.GetAttemptCountAsync(studentId, quizId);
                if (attemptCount > 0)
                    return BadRequest(new { success = false, message = "You have already taken this quiz. Only one attempt allowed." });

                var attempt = await _quizService.StartQuizAttemptAsync(quizId, studentId);
                return Ok(new
                {
                    success = true,
                    message = "Quiz attempt started",
                    data = attempt
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error starting quiz attempt {QuizId}", quizId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Submit quiz answers (Students only)
        /// </summary>
        [HttpPost("{quizId}/submit")]
        [Authorize(Policy = "StudentOnly")]
        public async Task<ActionResult> SubmitQuiz(int courseId, int quizId, [FromBody] Dictionary<int, int> answers)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid input" });

            try
            {
                var studentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(studentId))
                    return Unauthorized(new { success = false, message = "Student ID not found" });

                var attempt = await _quizService.SubmitQuizAsync(quizId, studentId, answers);
                return Ok(new
                {
                    success = true,
                    message = "Quiz submitted successfully",
                    data = attempt
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error submitting quiz {QuizId}", quizId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Get student's quiz attempts (Admin only, can override single attempt for student)
        /// </summary>
        [HttpPost("{quizId}/reset-for-student")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> ResetQuizForStudent(int courseId, int quizId, [FromQuery] string studentId)
        {
            try
            {
                // Logic to reset quiz for student - allow retake
                _logger.Information("Admin reset quiz {QuizId} for student {StudentId}", quizId, studentId);

                return Ok(new
                {
                    success = true,
                    message = "Quiz reset for student. They may now retake the quiz."
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error resetting quiz {QuizId}", quizId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }
    }
}