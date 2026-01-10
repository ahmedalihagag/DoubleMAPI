using AutoMapper;
using BLL.DTOs.LessonDTOs;
using BLL.Interfaces;
using DAL.Interfaces;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class LessonService : ILessonService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<LessonService> _logger;
        private readonly INotificationService _notificationService;
        private readonly IProgressService _progressService;

        public LessonService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<LessonService> logger,
            INotificationService notificationService,
            IProgressService progressService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _notificationService = notificationService;
            _progressService = progressService;
        }

        public async Task<LessonDto> CreateLessonAsync(CreateLessonDto createLessonDto)
        {
            _logger.LogInformation("Creating lesson: {Title}", createLessonDto.Title);

            var lesson = _mapper.Map<DAL.Entities.Lesson>(createLessonDto);

            await _unitOfWork.Lessons.AddAsync(lesson);
            await _unitOfWork.SaveChangesAsync();

            var section = await _unitOfWork.Sections.GetByIdAsync(createLessonDto.SectionId);
            if (section.HasValue)
            {
                var enrolledStudentIds = await _unitOfWork.CourseEnrollments
                    .GetEnrolledStudentIdsAsync(section.Value.CourseId);

                foreach (var studentId in enrolledStudentIds)
                {
                    await _notificationService.CreateNotificationAsync(
                        studentId,
                        "New Lesson Available",
                        $"A new lesson '{lesson.Title}' has been added to your course.",
                        "NewContent"
                    );
                }

                _logger.LogInformation("Notified {Count} students about new lesson: {Title}",
                    enrolledStudentIds.Count(), lesson.Title);
            }

            return _mapper.Map<LessonDto>(lesson);
        }

        public async Task<LessonDto?> GetLessonByIdAsync(int lessonId)
        {
            var lesson = await _unitOfWork.Lessons.GetByIdAsync(lessonId);
            return lesson == null ? null : _mapper.Map<LessonDto>(lesson);
        }

        public async Task<List<LessonDto>> GetLessonsBySectionAsync(int sectionId)
        {
            try
            {
                _logger.LogDebug("Getting lessons for section: {SectionId}", sectionId);

                var lessons = await _unitOfWork.Lessons.GetLessonsBySectionIdAsync(sectionId);

                var dtos = lessons
                    .Select(l => _mapper.Map<LessonDto>(l))
                    .ToList();

                _logger.LogInformation("Retrieved {Count} lessons for section: {SectionId}",
                    dtos.Count, sectionId);

                return dtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lessons for section: {SectionId}", sectionId);
                throw;
            }
        }

        public async Task<bool> UpdateLessonAsync(int lessonId, CreateLessonDto updateDto)
        {
            var lesson = await _unitOfWork.Lessons.GetByIdAsync(lessonId);
            if (lesson == null) return false;

            lesson.Title = updateDto.Title;
            lesson.VideoUrl = updateDto.VideoUrl;
            lesson.MaterialUrl = updateDto.MaterialUrl;
            lesson.Description = updateDto.Description;
            lesson.DisplayOrder = updateDto.DisplayOrder;
            lesson.DurationMinutes = updateDto.DurationMinutes;
            lesson.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Lessons.Update(lesson);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteLessonAsync(int lessonId)
        {
            var lesson = await _unitOfWork.Lessons.GetByIdAsync(lessonId);
            if (lesson == null) return false;

            lesson.IsDeleted = true;
            lesson.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Lessons.Update(lesson);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> MarkLessonCompleteAsync(string studentId, int lessonId)
        {
            return await _progressService.MarkLessonCompleteAsync(studentId, lessonId);
        }
    }
}
