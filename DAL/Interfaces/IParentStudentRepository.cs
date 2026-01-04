using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IParentStudentRepository : IRepository<ParentStudent>
    {
        Task<IEnumerable<ParentStudent>> GetStudentsByParentAsync(string parentId);
        Task<IEnumerable<ParentStudent>> GetParentsByStudentAsync(string studentId);
        Task<bool> IsLinkedAsync(string parentId, string studentId);
    }
}
