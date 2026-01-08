using DAL.Data;
using DAL.Entities;
using DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class DeviceSessionRepository : IDeviceSessionRepository
    {
        private readonly ApplicationDbContext _context;

        public DeviceSessionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DeviceSession> AddAsync(DeviceSession entity)
        {
            await _context.DeviceSessions.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(DeviceSession entity)
        {
            _context.DeviceSessions.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<List<DeviceSession>> GetAllAsync()
        {
            return await _context.DeviceSessions.ToListAsync();
        }

        public async Task<DeviceSession?> GetByIdAsync(int id)
        {
            return await _context.DeviceSessions.FindAsync(id);
        }

        public async Task<IEnumerable<DeviceSession>> FindAsync(Expression<Func<DeviceSession, bool>> predicate)
        {
            return await _context.DeviceSessions.Where(predicate).ToListAsync();
        }

        public async Task<DeviceSession> UpdateAsync(DeviceSession entity)
        {
            _context.DeviceSessions.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
    }
}
