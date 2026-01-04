using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IBlacklistRepository : IRepository<Blacklist>
    {
        Task<bool> IsUserBlockedAsync(string userId);
        Task<Blacklist?> GetActiveBlockAsync(string userId);
    }
}
