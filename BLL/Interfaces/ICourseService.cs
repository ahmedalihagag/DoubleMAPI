using DAL.Entities;
using DAL.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface ICourseService
    {
        Task<Course> CreateCourseAsync(Course course);
        Task<Course?> GetCourseByIdAsync(int courseId);
        Task<Course?> GetCourseWithDetailsAsync(int courseId);
        Task<PagedResult<Course>> GetPublishedCoursesAsync(PaginationParams paginationParams);
        Task<PagedResult<Course>> GetCoursesByTeacherAsync(string teacherId, PaginationParams paginationParams);
        Task<PagedResult<Course>> SearchCoursesAsync(string searchTerm, PaginationParams paginationParams);
        Task<bool> UpdateCourseAsync(Course course);
        Task<bool> DeleteCourseAsync(int courseId);
        Task<bool> PublishCourseAsync(int courseId);
    }
}
