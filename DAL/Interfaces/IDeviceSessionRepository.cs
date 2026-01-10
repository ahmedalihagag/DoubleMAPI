using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IDeviceSessionRepository : IRepository<DeviceSession>
    {
        Task<IEnumerable<DeviceSession>> FindAsync(Expression<Func<DeviceSession, bool>> predicate);
        Task<IEnumerable<DeviceSession>> FindAllAsync(Expression<Func<DeviceSession, bool>> predicate);
    }
}
