using AutoMapper;
using BLL.DTOs.EnrollmentDTOs;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class ProgressService : IProgressService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public ProgressService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = Log.ForContext<ProgressService>();
        }

        public async Task<bool> MarkLessonCompleteAsync(string studentId, int lessonId)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                _logger.Information("Marking lesson complete for student: {StudentId}, lesson: {LessonId}",
                    studentId, lessonId);

                var isCompleted = await _unitOfWork.LessonProgresses
                    .IsLessonCompletedAsync(studentId, lessonId);

                if (isCompleted)
                {
                    _logger.Warning("Lesson already completed");
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }

                var lesson = await _unitOfWork.Lessons.GetByIdAsync(lessonId);
                if (lesson == null)
                    throw new Exception("Lesson not found");

                var progress = new LessonProgress
                {
                    StudentId = studentId,
                    LessonId = lessonId,
                    CompletedAt = DateTime.UtcNow
                };

                await _unitOfWork.LessonProgresses.AddAsync(progress);

                // Update course progress
                var section = await _unitOfWork.Sections.GetByIdAsync(lesson.SectionId);
                if (section != null)
                {
                    await _unitOfWork.CourseProgresses
                        .UpdateCompletionPercentageAsync(studentId, section.CourseId);
                }

                await _unitOfWork.CommitTransactionAsync();
                _logger.Information("Lesson marked as complete");

                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.Error(ex, "Error marking lesson complete");
                throw;
            }
        }

        public async Task<CourseProgressDto?> GetCourseProgressAsync(string studentId, int courseId)
        {
            try
            {
                _logger.Debug("Getting course progress for student: {StudentId}, course: {CourseId}",
                    studentId, courseId);

                var progress = await _unitOfWork.CourseProgresses.GetProgressAsync(studentId, courseId);
                if (progress == null)
                    return null;

                var totalLessons = await _unitOfWork.Lessons
                    .CountAsync(l => l.Section.CourseId == courseId && !l.IsDeleted);

                var completedLessons = await _unitOfWork.LessonProgresses
                    .GetCompletedLessonsCountAsync(studentId, courseId);

                var dto = _mapper.Map<CourseProgressDto>(progress);
                dto.TotalLessons = totalLessons;
                dto.CompletedLessons = completedLessons;

                return dto;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting course progress");
                throw;
            }
        }

        public async Task<List<CourseProgressDto>> GetStudentProgressAsync(string studentId)
        {
            try
            {
                _logger.Debug("Getting all progress for student: {StudentId}", studentId);
                var progressList = await _unitOfWork.CourseProgresses.GetProgressByStudentAsync(studentId);
                var result = new List<CourseProgressDto>();

                foreach (var progress in progressList)
                {
                    var dto = _mapper.Map<CourseProgressDto>(progress);
                    dto.TotalLessons = await _unitOfWork.Lessons
                        .CountAsync(l => l.Section.CourseId == progress.CourseId && !l.IsDeleted);
                    dto.CompletedLessons = await _unitOfWork.LessonProgresses
                        .GetCompletedLessonsCountAsync(studentId, progress.CourseId);
                    result.Add(dto);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting student progress");
                throw;
            }
        }
    }

}
