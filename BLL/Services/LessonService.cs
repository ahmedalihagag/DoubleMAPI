using AutoMapper;
using BLL.DTOs.LessonDTOs;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
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
        private readonly ILogger<LessonService> _logger;

        public LessonService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<LessonService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<LessonDto> CreateLessonAsync(CreateLessonDto createLessonDto)
        {
            _logger.LogInformation("Creating lesson: {Title}", createLessonDto.Title);
            var lesson = _mapper.Map<Lesson>(createLessonDto);
            await _unitOfWork.Lessons.AddAsync(lesson);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<LessonDto>(lesson);
        }

        public async Task<LessonDto?> GetLessonByIdAsync(int lessonId)
        {
            var lesson = await _unitOfWork.Lessons
                .Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == lessonId && !l.IsDeleted);

            return lesson == null ? null : _mapper.Map<LessonDto>(lesson);
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
    }
}
