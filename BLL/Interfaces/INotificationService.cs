using BLL.DTOs.NotificationDTOs;
using DAL.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface INotificationService
    {
        Task<bool> CreateNotificationAsync(string userId, string title, string message, string type = "General");
        Task<PagedResult<NotificationDto>> GetUserNotificationsAsync(string userId, PaginationParams paginationParams);
        Task<PagedResult<NotificationDto>> GetUnreadNotificationsAsync(string userId, PaginationParams paginationParams);
        Task<int> GetUnreadCountAsync(string userId);
        Task<bool> MarkAsReadAsync(int notificationId);
        Task<bool> MarkAllAsReadAsync(string userId);
        Task<bool> DeleteNotificationAsync(int notificationId);
    }
}
