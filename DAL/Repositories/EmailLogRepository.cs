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
    public class EmailLogRepository : Repository<EmailLog>, IEmailLogRepository
    {
        public EmailLogRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<EmailLog>> GetFailedEmailsAsync()
        {
            try
            {
                return await _dbSet
                    .Where(el => el.Status == "Failed" && el.RetryCount < 3)
                    .OrderBy(el => el.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting failed emails");
                throw;
            }
        }

        public async Task<IEnumerable<EmailLog>> GetPendingEmailsAsync()
        {
            try
            {
                return await _dbSet
                    .Where(el => el.Status == "Pending")
                    .OrderBy(el => el.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting pending emails");
                throw;
            }
        }
    }
}
