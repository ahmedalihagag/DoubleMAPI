using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IParentLinkCodeRepository : IRepository<ParentLinkCode>
    {
        Task<ParentLinkCode?> GetByCodeAsync(string code);
        Task<IEnumerable<ParentLinkCode>> GetByStudentIdAsync(string studentId);
    }
}
