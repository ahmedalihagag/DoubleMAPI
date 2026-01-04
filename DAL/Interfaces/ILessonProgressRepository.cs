using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface ILessonProgressRepository : IRepository<LessonProgress>
    {
        Task<IEnumerable<LessonProgress>> GetProgressByStudentAsync(string studentId);
        Task<IEnumerable<LessonProgress>> GetProgressByCourseAsync(string studentId, int courseId);
        Task<bool> IsLessonCompletedAsync(string studentId, int lessonId);
        Task<int> GetCompletedLessonsCountAsync(string studentId, int courseId);
    }
}
