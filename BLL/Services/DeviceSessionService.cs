using DAL.Data;
using DAL.Entities;
using DAL.Enums;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class DeviceSessionService : IDeviceSessionService
    {
        private readonly ApplicationDbContext _context;

        public DeviceSessionService(ApplicationDbContext context)
        {
            _context = context;
        }

        // -------------------------
        // Validate and create a session
        // Ensures only 1 active session per client type
        // -------------------------
        public async Task<bool> ValidateAndCreateSessionAsync(
            string userId,
            string deviceId,
            ClientType clientType,
            string deviceInfo,
            string ipAddress)
        {
            // Deactivate any existing active session for this client type
            var existingSession = await _context.DeviceSessions
                .FirstOrDefaultAsync(s =>
                    s.UserId == userId &&
                    s.ClientType == clientType &&
                    s.IsActive);

            if (existingSession != null)
            {
                existingSession.IsActive = false;
                existingSession.LogoutAt = DateTime.UtcNow;
            }

            // Create new session
            var newSession = new DeviceSession
            {
                UserId = userId,
                DeviceId = deviceId,
                ClientType = clientType,
                DeviceInfo = deviceInfo,
                IPAddress = ipAddress,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _context.DeviceSessions.AddAsync(newSession);
            await _context.SaveChangesAsync();

            return true;
        }

        // -------------------------
        // Get active session for a specific user, client type, and device
        // -------------------------
        public async Task<DeviceSession?> GetActiveSessionAsync(int userId, ClientType clientType, string deviceId)
        {
            return await _context.DeviceSessions
                .Where(s =>
                    s.UserId == userId.ToString() &&
                    s.ClientType == clientType &&
                    s.DeviceId == deviceId &&
                    s.IsActive)
                .FirstOrDefaultAsync();
        }

        // -------------------------
        // Get all sessions for a user
        // -------------------------
        public async Task<List<DeviceSession>> GetUserSessionsAsync(string userId)
        {
            return await _context.DeviceSessions
                .Where(s => s.UserId == userId)
                .ToListAsync();
        }

        // -------------------------
        // Check if a device is allowed to log in
        // -------------------------
        public async Task<bool> IsDeviceAllowedAsync(string userId, string deviceId, ClientType clientType)
        {
            var activeSession = await _context.DeviceSessions
                .FirstOrDefaultAsync(s =>
                    s.UserId == userId &&
                    s.ClientType == clientType &&
                    s.IsActive);

            if (activeSession == null) return true;

            return activeSession.DeviceId == deviceId;
        }

        // -------------------------
        // Admin can reset all active sessions for a user
        // -------------------------
        public async Task<bool> AdminResetDeviceAsync(string userId, string adminId)
        {
            var sessions = await _context.DeviceSessions
                .Where(s => s.UserId == userId && s.IsActive)
                .ToListAsync();

            foreach (var session in sessions)
            {
                session.IsActive = false;
                session.LogoutAt = DateTime.UtcNow;
                session.DeactivatedByAdminId = adminId;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // -------------------------
        // Deactivate a session by client type
        // -------------------------
        public async Task DeactivateSessionAsync(string userId, ClientType clientType)
        {
            var sessions = await _context.DeviceSessions
                .Where(s => s.UserId == userId && s.ClientType == clientType && s.IsActive)
                .ToListAsync();

            foreach (var session in sessions)
            {
                session.IsActive = false;
                session.LogoutAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        // -------------------------
        // ------------------------- IRepository<DeviceSession> Methods -------------------------
        // -------------------------

        public async Task<DeviceSession> AddAsync(DeviceSession entity)
        {
            await _context.DeviceSessions.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(DeviceSession entity)
        {
            _context.DeviceSessions.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(DeviceSession entity)
        {
            _context.DeviceSessions.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<DeviceSession?> GetByIdAsync(int id)
        {
            return await _context.DeviceSessions.FindAsync(id);
        }

        public async Task<List<DeviceSession>> GetAllAsync()
        {
            return await _context.DeviceSessions.ToListAsync();
        }

        public async Task<IEnumerable<DeviceSession>> FindAsync(Func<DeviceSession, bool> predicate)
        {
            // In-memory filtering (OK for small datasets)
            return _context.DeviceSessions.AsEnumerable().Where(predicate);
        }
    }
}
