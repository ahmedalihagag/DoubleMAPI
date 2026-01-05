using AutoMapper;
using BLL.DTOs.LessonDTOs;
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
    // Lesson Service
    public class LessonService : ILessonService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public LessonService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = Log.ForContext<LessonService>();
        }

        public async Task<LessonDto> CreateLessonAsync(CreateLessonDto createLessonDto)
        {
            try
            {
                _logger.Information("Creating lesson: {Title} for section: {SectionId}",
                    createLessonDto.Title, createLessonDto.SectionId);

                var lesson = _mapper.Map<Lesson>(createLessonDto);
                await _unitOfWork.Lessons.AddAsync(lesson);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information("Lesson created: {LessonId}", lesson.Id);
                return _mapper.Map<LessonDto>(lesson);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating lesson");
                throw;
            }
        }

        public async Task<LessonDto?> GetLessonByIdAsync(int lessonId)
        {
            try
            {
                var lesson = await _unitOfWork.Lessons.GetByIdAsync(lessonId);
                return lesson == null ? null : _mapper.Map<LessonDto>(lesson);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting lesson: {LessonId}", lessonId);
                throw;
            }
        }

        public async Task<bool> UpdateLessonAsync(int lessonId, CreateLessonDto updateDto)
        {
            try
            {
                _logger.Information("Updating lesson: {LessonId}", lessonId);

                var lesson = await _unitOfWork.Lessons.GetByIdAsync(lessonId);
                if (lesson == null)
                    return false;

                lesson.Title = updateDto.Title;
                lesson.VideoUrl = updateDto.VideoUrl;
                lesson.MaterialUrl = updateDto.MaterialUrl;
                lesson.Description = updateDto.Description;
                lesson.DisplayOrder = updateDto.DisplayOrder;
                lesson.DurationMinutes = updateDto.DurationMinutes;
                lesson.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Lessons.Update(lesson);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information("Lesson updated: {LessonId}", lessonId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating lesson: {LessonId}", lessonId);
                throw;
            }
        }

        public async Task<bool> DeleteLessonAsync(int lessonId)
        {
            try
            {
                _logger.Information("Deleting lesson: {LessonId}", lessonId);

                var lesson = await _unitOfWork.Lessons.GetByIdAsync(lessonId);
                if (lesson == null)
                    return false;

                lesson.IsDeleted = true;
                lesson.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Lessons.Update(lesson);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information("Lesson soft-deleted: {LessonId}", lessonId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error deleting lesson: {LessonId}", lessonId);
                throw;
            }
        }
    }
}
}
