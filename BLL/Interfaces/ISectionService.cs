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
        Task<SectionDto> CreateSectionAsync(CreateSectionDto dto);
        Task<bool> UpdateSectionAsync(int sectionId, CreateSectionDto dto);
        Task<bool> DeleteSectionAsync(int sectionId);
        Task<SectionDto?> GetSectionByIdAsync(int sectionId);
        Task<IEnumerable<SectionDto>> GetSectionsByCourseAsync(int courseId);
    }
}
