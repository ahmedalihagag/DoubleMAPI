using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IRefreshTokenRepository : IRepository<RefreshToken>
    {
        Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);
        Task<IEnumerable<RefreshToken>> GetActiveTokensByUserAsync(string userId);
        Task RevokeAllUserTokensAsync(string userId);
        Task CleanupExpiredTokensAsync();
    }
}
