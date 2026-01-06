using AutoMapper;
using BLL.DTOs.NotificationDTOs;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;
using DAL.Pagination;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public NotificationService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = Log.ForContext<NotificationService>();
        }

        public async Task<bool> CreateNotificationAsync(
            string userId,
            string title,
            string message,
            string type = "General")
        {
            try
            {
                _logger.Information("Creating notification for user: {UserId}", userId);

                var notification = new Notification
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    Type = type,
                    Priority = "Medium",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Notifications.AddAsync(notification);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information("Notification created: {NotificationId} for user: {UserId}", notification.Id, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating notification for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<PagedResult<NotificationDto>> GetUserNotificationsAsync(
            string userId,
            PaginationParams paginationParams)
        {
            try
            {
                _logger.Debug("Getting notifications for user: {UserId}", userId);

                var pagedNotifications = await _unitOfWork.Notifications
                    .GetNotificationsByUserPagedAsync(userId, paginationParams);

                var dtos = _mapper.Map<IEnumerable<NotificationDto>>(pagedNotifications.Items);

                return new PagedResult<NotificationDto>(dtos, pagedNotifications.Metadata);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting notifications for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<PagedResult<NotificationDto>> GetUnreadNotificationsAsync(
            string userId,
            PaginationParams paginationParams)
        {
            try
            {
                _logger.Debug("Getting unread notifications for user: {UserId}", userId);

                var pagedNotifications = await _unitOfWork.Notifications
                    .GetUnreadNotificationsPagedAsync(userId, paginationParams);

                var dtos = _mapper.Map<IEnumerable<NotificationDto>>(pagedNotifications.Items);

                return new PagedResult<NotificationDto>(dtos, pagedNotifications.Metadata);
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
                var count = await _unitOfWork.Notifications.GetUnreadCountAsync(userId);
                _logger.Debug("User {UserId} has {Count} unread notifications", userId, count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting unread count for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> MarkAsReadAsync(int notificationId)
        {
            try
            {
                _logger.Debug("Marking notification as read: {NotificationId}", notificationId);

                await _unitOfWork.Notifications.MarkAsReadAsync(notificationId);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information("Notification marked as read: {NotificationId}", notificationId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error marking notification as read: {NotificationId}", notificationId);
                throw;
            }
        }

        public async Task<bool> MarkAllAsReadAsync(string userId)
        {
            try
            {
                _logger.Information("Marking all notifications as read for user: {UserId}", userId);

                await _unitOfWork.Notifications.MarkAllAsReadAsync(userId);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information("All notifications marked as read for user: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error marking all notifications as read for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> DeleteNotificationAsync(int notificationId)
        {
            try
            {
                _logger.Information("Deleting notification: {NotificationId}", notificationId);

                var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
                if (notification == null)
                {
                    _logger.Warning("Notification not found: {NotificationId}", notificationId);
                    return false;
                }

                _unitOfWork.Notifications.Delete(notification);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information("Notification deleted: {NotificationId}", notificationId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error deleting notification: {NotificationId}", notificationId);
                throw;
            }
        }
    }
}
