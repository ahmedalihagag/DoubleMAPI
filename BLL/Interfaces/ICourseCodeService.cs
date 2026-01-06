using BLL.DTOs.CourseDTOs;
using DAL.Entities;
using DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface ICourseCodeService
    {
        Task<CourseCodeDto> GenerateCodeAsync(CreateCourseCodeDto dto);
        Task<bool> DisableCodeAsync(string code);
        Task<bool> EnableCodeAsync(string code);
        Task<bool> UseCodeAsync(string code, string studentId);
        Task<CourseCodeDto?> GetByCodeAsync(string code);
        Task<IEnumerable<CourseCodeDto>> GetActiveCodesByCourseAsync(int courseId);
        Task<bool> UpdateCodeAsync(string code, UpdateCourseCodeDto dto);
    }
}
