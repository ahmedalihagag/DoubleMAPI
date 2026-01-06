using BLL.DTOs.EnrollmentDTOs;
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
        Task<PagedResult<EnrollmentDto>> GetStudentEnrollmentsAsync(string studentId, PaginationParams paginationParams);
        Task<PagedResult<EnrollmentDto>> GetCourseEnrollmentsAsync(int courseId, PaginationParams paginationParams);
        Task<bool> UnenrollStudentAsync(string studentId, int courseId);
        Task<bool> IsStudentEnrolledAsync(string studentId, int courseId);
        Task<int> GetEnrollmentCountAsync(int courseId);
    }
}
