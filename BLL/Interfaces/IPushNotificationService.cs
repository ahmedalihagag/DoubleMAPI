using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IPushNotificationService
    {
        Task SendToUserAsync(string userId, string title, string message);
        Task SendToMultipleUsersAsync(List<string> userIds, string title, string message);
    }
}
