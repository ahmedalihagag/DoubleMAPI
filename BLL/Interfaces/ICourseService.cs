using BLL.DTOs.CourseDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface ICourseService
    {
        Task<CourseDto> CreateCourseAsync(CreateCourseDto dto);
        Task<CourseDto?> GetCourseByIdAsync(int courseId);
        Task<List<CourseDto>> GetAllCoursesAsync();
        Task<bool> UpdateCourseAsync(int courseId, CreateCourseDto dto);
        Task<bool> DeleteCourseAsync(int courseId);
        Task<string> GenerateCourseAccessCodeAsync(int courseId, string adminId); // Only admin
    }
}
