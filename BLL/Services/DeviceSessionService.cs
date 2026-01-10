using BLL.Interfaces;
using DAL.Entities;
using DAL.Enums;
using DAL.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class DeviceSessionService : IDeviceSessionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;

        public DeviceSessionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _logger = Log.ForContext<DeviceSessionService>();
        }

        public async Task<DeviceSession?> GetActiveSessionAsync(string userId, ClientType clientType, string? deviceId = null)
        {
            try
            {
                _logger.Debug("Retrieving active session for user {UserId}, client type {ClientType}", userId, clientType);

                var sessions = await _unitOfWork.DeviceSessions.FindAsync(s =>
                    s.UserId == userId &&
                    s.ClientType == clientType &&
                    s.IsActive &&
                    s.ExpiresAt > DateTime.UtcNow);

                if (deviceId != null)
                    return sessions.FirstOrDefault(s => s.DeviceId == deviceId);

                return sessions.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting active session");
                throw;
            }
        }

        public async Task<DeviceSession> ValidateAndCreateSessionAsync(
            string userId,
            string deviceId,
            ClientType clientType,
            string deviceInfo,
            string ipAddress)
        {
            try
            {
                _logger.Information("Creating device session for user {UserId}, device {DeviceId}", userId, deviceId);

                var existingSession = await _unitOfWork.DeviceSessions.FindAsync(s =>
                    s.UserId == userId &&
                    s.DeviceId == deviceId &&
                    s.ClientType == clientType);

                DeviceSession session;

                if (existingSession != null)
                {
                    existingSession.LastActivityAt = DateTime.UtcNow;
                    existingSession.ExpiresAt = DateTime.UtcNow.AddDays(30);
                    existingSession.IsActive = true;
                    _unitOfWork.DeviceSessions.Update(existingSession);
                    session = existingSession;
                }
                else
                {
                    session = new DeviceSession
                    {
                        UserId = userId,
                        DeviceId = deviceId,
                        ClientType = clientType,
                        DeviceInfo = deviceInfo,
                        IpAddress = ipAddress,
                        CreatedAt = DateTime.UtcNow,
                        LastActivityAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddDays(30),
                        IsActive = true
                    };

                    await _unitOfWork.DeviceSessions.AddAsync(session);
                }

                await _unitOfWork.SaveChangesAsync();
                _logger.Information("Device session created/updated for user {UserId}", userId);

                return session;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating device session");
                throw;
            }
        }

        public async Task<bool> InvalidateSessionAsync(string userId, string deviceId)
        {
            try
            {
                _logger.Information("Invalidating session for user {UserId}, device {DeviceId}", userId, deviceId);

                var session = await _unitOfWork.DeviceSessions.FindAsync(s =>
                    s.UserId == userId && s.DeviceId == deviceId && s.IsActive);

                if (session == null)
                    return false;

                session.IsActive = false;
                _unitOfWork.DeviceSessions.Update(session);
                await _unitOfWork.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error invalidating session");
                throw;
            }
        }

        public async Task<bool> AdminResetDeviceAsync(string userId, string adminId)
        {
            try
            {
                _logger.Warning("Admin {AdminId} resetting device sessions for user {UserId}", adminId, userId);

                var sessions = await _unitOfWork.DeviceSessions.FindAsync(s =>
                    s.UserId == userId && s.IsActive);

                foreach (var session in sessions)
                {
                    session.IsActive = false;
                    _unitOfWork.DeviceSessions.Update(session);
                }

                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error resetting user devices");
                throw;
            }
        }

        public async Task<IEnumerable<DeviceSession>> GetUserSessionsAsync(string userId)
        {
            try
            {
                _logger.Debug("Getting all sessions for user {UserId}", userId);
                return await _unitOfWork.DeviceSessions.FindAsync(s =>
                    s.UserId == userId && s.IsActive && s.ExpiresAt > DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting user sessions");
                throw;
            }
        }
    }
}
