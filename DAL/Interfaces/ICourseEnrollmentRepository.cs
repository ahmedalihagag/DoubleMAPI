using DAL.Entities;
using DAL.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface ICourseEnrollmentRepository : IRepository<CourseEnrollment>
    {
        Task<PagedResult<CourseEnrollment>> GetEnrollmentsByStudentPagedAsync(string studentId, PaginationParams paginationParams);
        Task<PagedResult<CourseEnrollment>> GetEnrollmentsByCoursePagedAsync(int courseId, PaginationParams paginationParams);
        Task<bool> IsStudentEnrolledAsync(string studentId, int courseId);
        Task<int> GetEnrollmentCountByCourseAsync(int courseId);
    }

}
