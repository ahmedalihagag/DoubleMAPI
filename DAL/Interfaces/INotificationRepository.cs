using DAL.Entities;
using DAL.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface INotificationRepository : IRepository<Notification>
    {
        Task<PagedResult<Notification>> GetNotificationsByUserPagedAsync(string userId, PaginationParams paginationParams);
        Task<PagedResult<Notification>> GetUnreadNotificationsPagedAsync(string userId, PaginationParams paginationParams);
        Task<int> GetUnreadCountAsync(string userId);
        Task MarkAsReadAsync(int notificationId);
        Task MarkAllAsReadAsync(string userId);
    }
}
