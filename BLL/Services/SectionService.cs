using AutoMapper;
using BLL.DTOs.SectionDTOs;
using BLL.Interfaces;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    // Section Service
    public class SectionService : ISectionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SectionService> _logger;

        public SectionService(IUnitOfWork unitOfWork, ILogger<SectionService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<SectionDto> CreateAsync(CreateSectionDto dto)
        {
            var id = await _unitOfWork.Sections.CreateAsync(
                dto.CourseId,
                dto.Title,
                dto.DisplayOrder);

            _logger.LogInformation("Created Section Id {Id}", id);

            return new SectionDto
            {
                Id = id,
                CourseId = dto.CourseId,
                Title = dto.Title,
                DisplayOrder = dto.DisplayOrder
            };
        }

        public async Task<bool> UpdateAsync(int sectionId, CreateSectionDto dto)
        {
            var updated = await _unitOfWork.Sections.UpdateAsync(
                sectionId,
                dto.Title,
                dto.DisplayOrder);

            if (!updated)
                _logger.LogWarning("Section {Id} not found for update", sectionId);
            return updated;
        }

        public async Task<bool> DeleteAsync(int sectionId)
        {
            var deleted = await _unitOfWork.Sections.SoftDeleteAsync(sectionId);

            if (!deleted)
                _logger.LogWarning("Section {Id} not found for delete", sectionId);
            return deleted;
        }

        public async Task<SectionDto?> GetByIdAsync(int sectionId)
        {
            var data = await _unitOfWork.Sections.GetByIdAsync(sectionId);
            if (data == null) return null;

            return new SectionDto
            {
                Id = data.Value.Id,
                CourseId = data.Value.CourseId,
                Title = data.Value.Title,
                DisplayOrder = data.Value.DisplayOrder
            };
        }

        public async Task<IEnumerable<SectionDto>> GetAllByCourseIdAsync(int courseId)
        {
            var list = await _unitOfWork.Sections.GetAllByCourseIdAsync(courseId);

            return list.Select(s => new SectionDto
            {
                Id = s.Id,
                CourseId = s.CourseId,
                Title = s.Title,
                DisplayOrder = s.DisplayOrder
            });
        }
    }
}
