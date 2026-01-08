using BLL.Interfaces;
using DAL.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DoubleMAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        [HttpGet("my-notifications")]
        public async Task<IActionResult> GetMyNotifications([FromQuery] PaginationParams paginationParams)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var notifications = await _notificationService.GetUserNotificationsAsync(userId, paginationParams);
            return Ok(new { success = true, data = notifications });
        }

        [HttpPost("{notificationId}/mark-read")]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            await _notificationService.MarkAsReadAsync(notificationId);
            return Ok(new { success = true });
        }

        [HttpPost("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _notificationService.MarkAllAsReadAsync(userId);
            return Ok(new { success = true });
        }
    }
}
