using DAL.Entities;
using DAL.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IDeviceSessionService
    {
        Task<DeviceSession?> GetActiveSessionAsync(string userId, ClientType clientType, string? deviceId = null);
        Task<DeviceSession> ValidateAndCreateSessionAsync(string userId, string deviceId, ClientType clientType, string deviceInfo, string ipAddress);
        Task<bool> InvalidateSessionAsync(string userId, string deviceId);
        Task<bool> AdminResetDeviceAsync(string userId, string adminId);
        Task<IEnumerable<DeviceSession>> GetUserSessionsAsync(string userId);
    }
}
