using DAL.Data;
using DAL.Entities;
using DAL.Interfaces;
using DAL.Pagination;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class NotificationRepository : Repository<Notification>, INotificationRepository
    {
        public NotificationRepository(ApplicationDbContext context) : base(context) { }

        public async Task<PagedResult<Notification>> GetNotificationsByUserPagedAsync(
            string userId,
            PaginationParams paginationParams)
        {
            try
            {
                _logger.Debug("Getting notifications for user: {UserId}", userId);
                return await GetPagedAsync(paginationParams, n => n.UserId == userId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting notifications for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<PagedResult<Notification>> GetUnreadNotificationsPagedAsync(
            string userId,
            PaginationParams paginationParams)
        {
            try
            {
                _logger.Debug("Getting unread notifications for user: {UserId}", userId);
                return await GetPagedAsync(paginationParams, n => n.UserId == userId && !n.IsRead);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting unread notifications for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            try
            {
                return await _dbSet.CountAsync(n => n.UserId == userId && !n.IsRead);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error counting unread notifications for user: {UserId}", userId);
                throw;
            }
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            try
            {
                var notification = await _dbSet.FindAsync(notificationId);
                if (notification != null)
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.UtcNow;
                    _logger.Information("Marked notification {NotificationId} as read", notificationId);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error marking notification as read: {NotificationId}", notificationId);
                throw;
            }
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            try
            {
                var notifications = await _dbSet
                    .Where(n => n.UserId == userId && !n.IsRead)
                    .ToListAsync();

                foreach (var notification in notifications)
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.UtcNow;
                }

                _logger.Information("Marked {Count} notifications as read for user: {UserId}",
                    notifications.Count, userId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error marking all notifications as read for user: {UserId}", userId);
                throw;
            }
        }
    }
}
