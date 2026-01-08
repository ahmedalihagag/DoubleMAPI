using AutoMapper;
using BLL.DTOs.QuizAttemptDTOs;
using BLL.DTOs.QuizDTOs;
using BLL.Interfaces;
using DAL.Interfaces;
using DAL.Pagination;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class QuizService : IQuizService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService; // ✅ Changed from IEmailService
        private readonly Serilog.ILogger _logger;

        public QuizService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            INotificationService notificationService) // ✅ Changed parameter
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService; // ✅ Inject notification service
            _logger = Log.ForContext<QuizService>();
        }

        public async Task<QuizDetailDto> CreateQuizAsync(CreateQuizDto createQuizDto)
        {
            try
            {
                _logger.Information("Creating quiz: {Title} for course: {CourseId}",
                    createQuizDto.Title, createQuizDto.CourseId);

                // ✅ Map DTO to Entity
                var quiz = _mapper.Map<DAL.Entities.Quiz>(createQuizDto);
                await _unitOfWork.Quizzes.AddAsync(quiz);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<QuizDetailDto>(quiz);
                _logger.Information("Quiz created successfully: {QuizId}", quiz.Id);

                // ✅ Notify all enrolled students about new quiz
                var enrolledStudentIds = await _unitOfWork.CourseEnrollments
                    .GetEnrolledStudentIdsAsync(createQuizDto.CourseId);

                foreach (var studentId in enrolledStudentIds)
                {
                    await _notificationService.CreateNotificationAsync(
                        studentId,
                        "New Quiz Available",
                        $"A new quiz '{createQuizDto.Title}' has been added to your course.",
                        "NewQuiz"
                    );
                }

                _logger.Information("Notified {Count} students about new quiz: {Title}",
                    enrolledStudentIds.Count(), createQuizDto.Title);

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating quiz: {Title}", createQuizDto.Title);
                throw;
            }
        }

        public async Task<QuizDetailDto?> GetQuizWithQuestionsAsync(int quizId)
        {
            try
            {
                _logger.Debug("Getting quiz with questions: {QuizId}", quizId);
                var quiz = await _unitOfWork.Quizzes.GetQuizWithQuestionsAsync(quizId);

                if (quiz == null)
                {
                    _logger.Warning("Quiz not found: {QuizId}", quizId);
                    return null;
                }

                return _mapper.Map<QuizDetailDto>(quiz);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting quiz: {QuizId}", quizId);
                throw;
            }
        }

        public async Task<QuizAttemptDto> StartQuizAttemptAsync(int quizId, string studentId)
        {
            try
            {
                _logger.Information("Starting quiz attempt for student: {StudentId}, quiz: {QuizId}",
                    studentId, quizId);

                var quiz = await _unitOfWork.Quizzes.GetByIdAsync(quizId);
                if (quiz == null)
                    throw new Exception("Quiz not found");

                // Check attempt limit
                var attemptCount = await _unitOfWork.QuizAttempts.GetAttemptCountAsync(studentId, quizId);
                if (quiz.AttemptsAllowed > 0 && attemptCount >= quiz.AttemptsAllowed)
                {
                    _logger.Warning("Max attempts reached for student: {StudentId}, quiz: {QuizId}",
                        studentId, quizId);
                    throw new Exception("Maximum attempts reached");
                }

                var attempt = _mapper.Map<DAL.Entities.QuizAttempt>(new
                {
                    QuizId = quizId,
                    StudentId = studentId,
                    StartedAt = DateTime.UtcNow
                });

                await _unitOfWork.QuizAttempts.AddAsync(attempt);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<QuizAttemptDto>(attempt);
                _logger.Information("Quiz attempt started: {AttemptId}", attempt.Id);

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error starting quiz attempt");
                throw;
            }
        }

        public async Task<QuizAttemptDto> SubmitQuizAsync(int quizId, string studentId, Dictionary<int, int> answers)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                _logger.Information("Submitting quiz: {QuizId} for student: {StudentId}", quizId, studentId);

                var quiz = await _unitOfWork.Quizzes.GetQuizWithQuestionsAsync(quizId);
                if (quiz == null)
                    throw new Exception("Quiz not found");

                var attempt = new DAL.Entities.QuizAttempt
                {
                    QuizId = quizId,
                    StudentId = studentId,
                    StartedAt = DateTime.UtcNow,
                    SubmittedAt = DateTime.UtcNow
                };

                decimal totalScore = 0;
                decimal maxScore = 0;

                foreach (var question in quiz.Questions)
                {
                    maxScore += question.Points;

                    if (answers.TryGetValue(question.Id, out var selectedOptionId))
                    {
                        var option = question.Options.FirstOrDefault(o => o.Id == selectedOptionId);
                        var isCorrect = option?.IsCorrect ?? false;
                        var pointsEarned = isCorrect ? question.Points : 0;

                        totalScore += pointsEarned;

                        var studentAnswer = new DAL.Entities.StudentAnswer
                        {
                            QuizAttemptId = attempt.Id,
                            QuestionId = question.Id,
                            SelectedOptionId = selectedOptionId,
                            IsCorrect = isCorrect,
                            PointsEarned = pointsEarned
                        };

                        await _unitOfWork.StudentAnswers.AddAsync(studentAnswer);
                    }
                }

                attempt.Score = totalScore;
                attempt.MaxScore = maxScore;
                attempt.Percentage = maxScore > 0 ? (totalScore / maxScore) * 100 : 0;
                attempt.IsPassed = quiz.PassingScore.HasValue && attempt.Percentage >= quiz.PassingScore.Value;
                attempt.TimeSpentMinutes = (int)(DateTime.UtcNow - attempt.StartedAt).TotalMinutes;

                await _unitOfWork.QuizAttempts.AddAsync(attempt);

                // ✅ Create notification for quiz completion
                await _notificationService.CreateNotificationAsync(
                    studentId,
                    "Quiz Completed",
                    $"You scored {attempt.Percentage:F1}% on {quiz.Title}",
                    "QuizCompleted"
                );

                await _unitOfWork.CommitTransactionAsync();

                _logger.Information("Quiz submitted successfully - Score: {Score}/{MaxScore}", totalScore, maxScore);
                return _mapper.Map<QuizAttemptDto>(attempt);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.Error(ex, "Error submitting quiz");
                throw;
            }
        }

        public async Task<PagedResult<QuizAttemptDto>> GetStudentAttemptsAsync(
            string studentId,
            PaginationParams paginationParams)
        {
            try
            {
                _logger.Debug("Getting quiz attempts for student: {StudentId}", studentId);
                var pagedAttempts = await _unitOfWork.QuizAttempts
                    .GetAttemptsByStudentPagedAsync(studentId, paginationParams);

                var dtos = _mapper.Map<IEnumerable<QuizAttemptDto>>(pagedAttempts.Items);
                return new PagedResult<QuizAttemptDto>(dtos, pagedAttempts.Metadata);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting student attempts");
                throw;
            }
        }

        public async Task<int> GetAttemptCountAsync(string studentId, int quizId)
        {
            try
            {
                return await _unitOfWork.QuizAttempts.GetAttemptCountAsync(studentId, quizId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting attempt count");
                throw;
            }
        }

        public async Task<IEnumerable<QuizAttemptDto>> GetAttemptsByStudentAndQuizAsync(string studentId, int quizId)
        {
            try
            {
                _logger.Information("Getting quiz attempts for student {StudentId} and quiz {QuizId}", studentId, quizId);

                var attempts = await _unitOfWork.QuizAttempts.GetAttemptsByStudentAndQuizAsync(studentId, quizId);
                var attemptDtos = _mapper.Map<IEnumerable<QuizAttemptDto>>(attempts);

                _logger.Information("Retrieved {Count} attempts for student {StudentId} and quiz {QuizId}",
                    attemptDtos.Count(), studentId, quizId);

                return attemptDtos;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting attempts for student {StudentId} and quiz {QuizId}", studentId, quizId);
                throw;
            }
        }

        public async Task<bool> DeleteAttemptAsync(int attemptId)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                _logger.Information("Deleting quiz attempt {AttemptId}", attemptId);

                var attempt = await _unitOfWork.QuizAttempts.GetByIdAsync(attemptId);
                if (attempt == null)
                {
                    _logger.Warning("Quiz attempt not found: {AttemptId}", attemptId);
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }

                // Delete associated student answers first
                var studentAnswers = await _unitOfWork.StudentAnswers.GetByQuizAttemptAsync(attemptId);
                foreach (var answer in studentAnswers)
                {
                    _unitOfWork.StudentAnswers.Delete(answer);
                }

                // Delete the attempt
                _unitOfWork.QuizAttempts.Delete(attempt);

                await _unitOfWork.CommitTransactionAsync();

                _logger.Information("Quiz attempt {AttemptId} deleted successfully", attemptId);
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.Error(ex, "Error deleting quiz attempt {AttemptId}", attemptId);
                throw;
            }
        }
    }
}