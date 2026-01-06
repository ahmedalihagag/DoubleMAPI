using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface ILessonRepository : IRepository<Lesson>
    {
        // Lesson-specific queries
        Task<Lesson?> GetLessonWithSectionsAsync(int lessonId);
        IQueryable<Lesson> GetLessonsByCourseId(int courseId);

        Task<int> GetCompletedLessonsCountAsync(string studentId, int courseId);
        Task<bool> IsLessonCompletedAsync(string studentId, int lessonId);
    }
}
