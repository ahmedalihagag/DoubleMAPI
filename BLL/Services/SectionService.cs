using AutoMapper;
using BLL.DTOs.SectionDTOs;
using BLL.Interfaces;
using DAL.Interfaces;
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
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public SectionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = Log.ForContext<SectionService>();
        }

        public async Task<SectionDto> CreateSectionAsync(CreateSectionDto createSectionDto)
        {
            try
            {
                _logger.Information("Creating section: {Title} for course: {CourseId}",
                    createSectionDto.Title, createSectionDto.CourseId);

                var section = _mapper.Map<Section>(createSectionDto);
                await _unitOfWork.Sections.AddAsync(section);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information("Section created: {SectionId}", section.Id);
                return _mapper.Map<SectionDto>(section);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating section");
                throw;
            }
        }

        public async Task<SectionDto?> GetSectionByIdAsync(int sectionId)
        {
            try
            {
                var section = await _unitOfWork.Sections.GetByIdAsync(sectionId);
                return section == null ? null : _mapper.Map<SectionDto>(section);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting section: {SectionId}", sectionId);
                throw;
            }
        }

        public async Task<bool> UpdateSectionAsync(int sectionId, CreateSectionDto updateDto)
        {
            try
            {
                _logger.Information("Updating section: {SectionId}", sectionId);

                var section = await _unitOfWork.Sections.GetByIdAsync(sectionId);
                if (section == null)
                    return false;

                section.Title = updateDto.Title;
                section.DisplayOrder = updateDto.DisplayOrder;
                section.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Sections.Update(section);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information("Section updated: {SectionId}", sectionId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating section: {SectionId}", sectionId);
                throw;
            }
        }

        public async Task<bool> DeleteSectionAsync(int sectionId)
        {
            try
            {
                _logger.Information("Deleting section: {SectionId}", sectionId);

                var section = await _unitOfWork.Sections.GetByIdAsync(sectionId);
                if (section == null)
                    return false;

                _unitOfWork.Sections.Delete(section);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information("Section deleted: {SectionId}", sectionId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error deleting section: {SectionId}", sectionId);
                throw;
            }
        }
    }
}
