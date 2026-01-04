using DAL.Entities;
using DAL.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IEnrollmentService
    {
        Task<bool> EnrollStudentAsync(string studentId, int courseId);
        Task<bool> EnrollStudentByCodeAsync(string studentId, string courseCode);
        Task<PagedResult<CourseEnrollment>> GetStudentEnrollmentsAsync(string studentId, PaginationParams paginationParams);
        Task<PagedResult<CourseEnrollment>> GetCourseEnrollmentsAsync(int courseId, PaginationParams paginationParams);
        Task<bool> UnenrollStudentAsync(string studentId, int courseId);
    }
}
