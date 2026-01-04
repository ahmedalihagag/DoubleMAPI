using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface ICourseProgressRepository : IRepository<CourseProgress>
    {
        Task<CourseProgress?> GetProgressAsync(string studentId, int courseId);
        Task<IEnumerable<CourseProgress>> GetProgressByStudentAsync(string studentId);
        Task UpdateCompletionPercentageAsync(string studentId, int courseId);
    }
}
