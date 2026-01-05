using BLL.DTOs.EnrollmentDTOs;
using BLL.DTOs.ParentStudentDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IParentService
    {
        Task<string> GenerateLinkCodeAsync(string studentId);
        Task<bool> LinkParentToStudentAsync(string parentId, string code);
        Task<List<StudentInfoDto>> GetLinkedStudentsAsync(string parentId);
        Task<List<CourseProgressDto>> GetStudentProgressAsync(string parentId, string studentId);
    }
}
