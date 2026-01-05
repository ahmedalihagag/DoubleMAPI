using BLL.DTOs.EnrollmentDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IProgressService
    {
        Task<bool> MarkLessonCompleteAsync(string studentId, int lessonId);
        Task<CourseProgressDto?> GetCourseProgressAsync(string studentId, int courseId);
        Task<List<CourseProgressDto>> GetStudentProgressAsync(string studentId);
    }
}
