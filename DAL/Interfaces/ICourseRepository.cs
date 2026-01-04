using DAL.Entities;
using DAL.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface ICourseRepository : IRepository<Course>
    {
        Task<PagedResult<Course>> GetPublishedCoursesPagedAsync(PaginationParams paginationParams);
        Task<PagedResult<Course>> GetCoursesByTeacherPagedAsync(string teacherId, PaginationParams paginationParams);
        Task<PagedResult<Course>> SearchCoursesPagedAsync(string searchTerm, PaginationParams paginationParams);
        Task<Course?> GetCourseWithSectionsAndLessonsAsync(int courseId);
        Task<IEnumerable<Course>> GetCoursesByCategoryAsync(string category);
    }
}
