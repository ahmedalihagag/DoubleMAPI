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
        Task<SectionDto> CreateAsync(CreateSectionDto dto);
        Task<bool> UpdateAsync(int sectionId, CreateSectionDto dto);
        Task<bool> DeleteAsync(int sectionId);
        Task<SectionDto?> GetByIdAsync(int sectionId);
        Task<IEnumerable<SectionDto>> GetAllByCourseIdAsync(int courseId);
    }
}
