using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface ICourseAccessCodeRepository : IRepository<CourseAccessCode>
    {
        IQueryable<CourseAccessCode> Query();
        Task<IEnumerable<CourseAccessCode>> GetByCourseIdAsync(int courseId);
        Task DisableCodeAsync(string code, string disabledBy);
    }
}
