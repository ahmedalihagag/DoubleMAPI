using DAL.Data;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(ApplicationDbContext context) : base(context) { }

        public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
        {
            try
            {
                return await _dbSet
                    .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash && !rt.IsRevoked);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting refresh token by hash");
                throw;
            }
        }

        public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserAsync(string userId)
        {
            try
            {
                return await _dbSet
                    .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting active tokens for user: {UserId}", userId);
                throw;
            }
        }

        public async Task RevokeAllUserTokensAsync(string userId)
        {
            try
            {
                _logger.Information("Revoking all tokens for user: {UserId}", userId);
                var tokens = await _dbSet
                    .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                    .ToListAsync();

                foreach (var token in tokens)
                {
                    token.IsRevoked = true;
                    token.RevokedAt = DateTime.UtcNow;
                }

                _logger.Information("Revoked {Count} tokens for user: {UserId}", tokens.Count, userId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error revoking tokens for user: {UserId}", userId);
                throw;
            }
        }

        public async Task CleanupExpiredTokensAsync()
        {
            try
            {
                _logger.Information("Cleaning up expired tokens");
                var expiredTokens = await _dbSet
                    .Where(rt => rt.ExpiresAt < DateTime.UtcNow)
                    .ToListAsync();

                _dbSet.RemoveRange(expiredTokens);
                _logger.Information("Cleaned up {Count} expired tokens", expiredTokens.Count);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error cleaning up expired tokens");
                throw;
            }
        }
    }
}
