using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IEmailLogRepository : IRepository<EmailLog>
    {
        Task<IEnumerable<EmailLog>> GetFailedEmailsAsync();
        Task<IEnumerable<EmailLog>> GetPendingEmailsAsync();
    }
}
