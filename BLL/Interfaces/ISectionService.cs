using BLL.DTOs.SectionDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface ISectionService
    {
        Task<SectionDto> CreateSectionAsync(CreateSectionDto createSectionDto);
        Task<SectionDto?> GetSectionByIdAsync(int sectionId);
        Task<bool> UpdateSectionAsync(int sectionId, CreateSectionDto updateDto);
        Task<bool> DeleteSectionAsync(int sectionId);
    }
}
