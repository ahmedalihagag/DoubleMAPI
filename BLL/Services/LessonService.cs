using AutoMapper;
using BLL.DTOs.LessonDTOs;
using BLL.Interfaces;
using DAL.Interfaces;
using Microsoft.Extensions.Logging;
using System;
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

        public LessonService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<LessonService> logger,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _notificationService = notificationService;
        }

        public async Task<LessonDto> CreateLessonAsync(CreateLessonDto createLessonDto)
        {
            _logger.LogInformation("Creating lesson: {Title}", createLessonDto.Title);

            // ✅ Map DTO to Entity in BLL
            var lesson = _mapper.Map<DAL.Entities.Lesson>(createLessonDto);

            // ✅ Use UnitOfWork to add entity
            await _unitOfWork.Lessons.AddAsync(lesson);
            await _unitOfWork.SaveChangesAsync();

            // ✅ Get section info to find course ID
            var section = await _unitOfWork.Sections.GetByIdAsync(createLessonDto.SectionId);
            if (section.HasValue)
            {
                // ✅ Get enrolled student IDs (primitives, not entities)
                var enrolledStudentIds = await _unitOfWork.CourseEnrollments
                    .GetEnrolledStudentIdsAsync(section.Value.CourseId);

                // ✅ Create notifications for each student
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

            // ✅ Map entity back to DTO in BLL
            return _mapper.Map<LessonDto>(lesson);
        }

        public async Task<LessonDto?> GetLessonByIdAsync(int lessonId)
        {
            // ✅ Get entity from DAL
            var lesson = await _unitOfWork.Lessons.GetByIdAsync(lessonId);

            // ✅ Map to DTO in BLL
            return lesson == null ? null : _mapper.Map<LessonDto>(lesson);
        }

        public async Task<bool> UpdateLessonAsync(int lessonId, CreateLessonDto updateDto)
        {
            // ✅ Get entity from DAL
            var lesson = await _unitOfWork.Lessons.GetByIdAsync(lessonId);
            if (lesson == null) return false;

            // ✅ Update entity properties in BLL
            lesson.Title = updateDto.Title;
            lesson.VideoUrl = updateDto.VideoUrl;
            lesson.MaterialUrl = updateDto.MaterialUrl;
            lesson.Description = updateDto.Description;
            lesson.DisplayOrder = updateDto.DisplayOrder;
            lesson.DurationMinutes = updateDto.DurationMinutes;
            lesson.UpdatedAt = DateTime.UtcNow;

            // ✅ Use UnitOfWork to update
            _unitOfWork.Lessons.Update(lesson);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteLessonAsync(int lessonId)
        {
            // ✅ Get entity from DAL
            var lesson = await _unitOfWork.Lessons.GetByIdAsync(lessonId);
            if (lesson == null) return false;

            // ✅ Soft delete in BLL
            lesson.IsDeleted = true;
            lesson.UpdatedAt = DateTime.UtcNow;

            // ✅ Use UnitOfWork to update
            _unitOfWork.Lessons.Update(lesson);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}