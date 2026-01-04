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
    public class BlacklistRepository : Repository<Blacklist>, IBlacklistRepository
    {
        public BlacklistRepository(ApplicationDbContext context) : base(context) { }

        public async Task<bool> IsUserBlockedAsync(string userId)
        {
            try
            {
                return await _dbSet.AnyAsync(b =>
                    b.UserId == userId &&
                    b.UnblockedAt == null &&
                    (b.BlockType == "Permanent" || b.ExpiresAt > DateTime.UtcNow));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error checking if user is blocked: {UserId}", userId);
                throw;
            }
        }

        public async Task<Blacklist?> GetActiveBlockAsync(string userId)
        {
            try
            {
                return await _dbSet
                    .Where(b => b.UserId == userId &&
                        b.UnblockedAt == null &&
                        (b.BlockType == "Permanent" || b.ExpiresAt > DateTime.UtcNow))
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting active block for user: {UserId}", userId);
                throw;
            }
        }
    }
}
