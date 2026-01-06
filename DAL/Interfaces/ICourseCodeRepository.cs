using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface ICourseCodeRepository : IRepository<CourseCode>
    {
        Task<CourseCode> GenerateCodeAsync(int courseId, string issuedBy, DateTime expiresAt);
        Task<CourseCode?> GetByCodeAsync(string code);

        Task<bool> DisableCodeAsync(string code);
        Task<bool> EnableCodeAsync(string code);
        Task<IEnumerable<CourseCode>> GetActiveCodesByCourseIdAsync(int courseId);

    }
}
