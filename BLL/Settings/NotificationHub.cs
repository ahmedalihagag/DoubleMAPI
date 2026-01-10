using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Serilog;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BLL.Settings;

/// <summary>
/// SignalR Hub for real-time notifications
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger _logger;

    public NotificationHub()
    {
        _logger = Log.ForContext<NotificationHub>();
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _logger.Information("User {UserId} connected to NotificationHub. ConnectionId: {ConnectionId}",
            userId, Context.ConnectionId);

        // Add user to a group named after their ID for targeted notifications
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            _logger.Information("User {UserId} added to group: user_{UserId}", userId, userId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _logger.Information("User {UserId} disconnected from NotificationHub. ConnectionId: {ConnectionId}",
            userId, Context.ConnectionId);

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Send notification to specific user
    /// </summary>
    public async Task SendNotificationToUser(string userId, string title, string message)
    {
        try
        {
            _logger.Debug("Sending notification to user: {UserId}", userId);

            await Clients.Group($"user_{userId}")
                .SendAsync("ReceiveNotification", new
                {
                    title,
                    message,
                    timestamp = DateTime.UtcNow
                });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error sending notification to user {UserId}", userId);
        }
    }

    /// <summary>
    /// Send notification to all connected clients
    /// </summary>
    public async Task SendNotificationToAll(string title, string message)
    {
        try
        {
            _logger.Debug("Sending notification to all users");

            await Clients.All.SendAsync("ReceiveNotification", new
            {
                title,
                message,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error sending notification to all");
        }
    }

    /// <summary>
    /// Send notification to a group (e.g., all students in a course)
    /// </summary>
    public async Task SendNotificationToGroup(string groupName, string title, string message)
    {
        try
        {
            _logger.Debug("Sending notification to group: {GroupName}", groupName);

            await Clients.Group(groupName).SendAsync("ReceiveNotification", new
            {
                title,
                message,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error sending notification to group {GroupName}", groupName);
        }
    }
}
