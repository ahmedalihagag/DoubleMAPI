using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Settings
{
    // Missing: NotificationHub.cs
    public class NotificationHub : Hub
    {

        public async Task SendNotificationToAll(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveNotification", user, message);
        }
        public async Task SendNotificationToUser(string userId, string message)
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", message);
        }
    }
}
