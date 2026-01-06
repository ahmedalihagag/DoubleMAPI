using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface ISectionRepository
    {
        // Commands
        Task<int> CreateAsync(int courseId, string title, int displayOrder);
        Task<bool> UpdateAsync(int sectionId, string title, int displayOrder);
        Task<bool> SoftDeleteAsync(int sectionId);

        // Queries (return primitives only)
        Task<(int Id, int CourseId, string Title, int DisplayOrder)?> GetByIdAsync(int sectionId);
        Task<IEnumerable<(int Id, int CourseId, string Title, int DisplayOrder)>> GetAllByCourseIdAsync(int courseId);
    }
}
