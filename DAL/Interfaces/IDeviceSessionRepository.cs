using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IDeviceSessionRepository : IRepository<DeviceSession>
    {
        Task<DeviceSession> AddAsync(DeviceSession entity);
        Task<DeviceSession> UpdateAsync(DeviceSession entity);
        Task DeleteAsync(DeviceSession entity);
        Task<DeviceSession?> GetByIdAsync(int id);
        Task<IEnumerable<DeviceSession>> FindAsync(Expression<Func<DeviceSession, bool>> predicate);
        Task<List<DeviceSession>> GetAllAsync();
    }
}
