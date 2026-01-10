using DAL.Data;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class DeviceSessionRepository : Repository<DeviceSession>, IDeviceSessionRepository
    {
        private readonly ILogger _logger;

        public DeviceSessionRepository(ApplicationDbContext context) : base(context)
        {
            _logger = Log.ForContext<DeviceSessionRepository>();
        }

        public async Task<IEnumerable<DeviceSession>> FindAsync(Expression<Func<DeviceSession, bool>> predicate)
        {
            try
            {
                _logger.Debug("Finding device sessions with predicate");
                return await _dbSet.Where(predicate).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error finding device sessions");
                throw;
            }
        }

        public async Task<IEnumerable<DeviceSession>> FindAllAsync(Expression<Func<DeviceSession, bool>> predicate)
        {
            try
            {
                _logger.Debug("Finding all device sessions with predicate");
                return await _dbSet.Where(predicate).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error finding all device sessions");
                throw;
            }
        }
    }
}
