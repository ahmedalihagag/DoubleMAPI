using BLL.DTOs.UserDTOs;
using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DoubleMAPI.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminUserManagementController : ControllerBase
    {
        private readonly IAdminUserService _adminUserService;
        private readonly Serilog.ILogger _logger;

        public AdminUserManagementController(IAdminUserService adminUserService)
        {
            _adminUserService = adminUserService ?? throw new ArgumentNullException(nameof(adminUserService));
            _logger = Log.ForContext<AdminUserManagementController>();
        }

        /// <summary>
        /// Create a new admin user
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreateAdminUser([FromBody] CreateAdminUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid input", errors = ModelState.Values });

            try
            {
                var createdBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(createdBy))
                    return Unauthorized(new { success = false, message = "Admin ID not found" });

                var success = await _adminUserService.CreateAdminUserAsync(dto);
                if (!success)
                    return BadRequest(new { success = false, message = "Failed to create admin user. Email may already exist." });

                _logger.Information("Admin user created by {CreatedBy}: {Email}", createdBy, dto.Email);
                return Ok(new { success = true, message = "Admin user created successfully", email = dto.Email });
            }
            catch (ArgumentException ex)
            {
                _logger.Warning(ex, "Invalid argument: {Message}", ex.Message);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating admin user: {Email}", dto.Email);
                return StatusCode(500, new { success = false, message = "An error occurred while creating admin user" });
            }
        }

        /// <summary>
        /// Update admin user information
        /// </summary>
        [HttpPut("update/{userId}")]
        public async Task<IActionResult> UpdateAdminUser(string userId, [FromBody] UpdateAdminUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid input", errors = ModelState.Values });

            try
            {
                var updatedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(updatedBy))
                    return Unauthorized(new { success = false, message = "Admin ID not found" });

                var success = await _adminUserService.UpdateAdminUserAsync(userId, dto);
                if (!success)
                    return BadRequest(new { success = false, message = "Failed to update admin user or user does not exist" });

                _logger.Information("Admin user updated by {UpdatedBy}: {UserId}", updatedBy, userId);
                return Ok(new { success = true, message = "Admin user updated successfully" });
            }
            catch (ArgumentException ex)
            {
                _logger.Warning(ex, "Invalid argument: {Message}", ex.Message);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating admin user: {UserId}", userId);
                return StatusCode(500, new { success = false, message = "An error occurred while updating admin user" });
            }
        }

        /// <summary>
        /// Get all admin users
        /// </summary>
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllAdmins()
        {
            try
            {
                var admins = await _adminUserService.GetAllAdminsAsync();
                return Ok(new { success = true, count = admins.Count(), data = admins });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error retrieving all admin users");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving admin users" });
            }
        }

        /// <summary>
        /// Get admin users with pagination
        /// </summary>
        [HttpGet("paged")]
        public async Task<IActionResult> GetAdminsPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (pageNumber < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Page number and page size must be greater than 0" });

                var (admins, totalCount) = await _adminUserService.GetAdminsPagedAsync(pageNumber, pageSize);
                var totalPages = (totalCount + pageSize - 1) / pageSize;

                return Ok(new
                {
                    success = true,
                    data = admins,
                    pagination = new
                    {
                        pageNumber,
                        pageSize,
                        totalCount,
                        totalPages
                    }
                });
            }
            catch (ArgumentException ex)
            {
                _logger.Warning(ex, "Invalid argument: {Message}", ex.Message);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error retrieving paged admin users");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving admin users" });
            }
        }

        /// <summary>
        /// Get admin user by ID
        /// </summary>
        [HttpGet("{userId}")]
        public async Task<ActionResult<UserDto>> GetAdminById(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return BadRequest(new { success = false, message = "User ID cannot be empty" });

                var admin = await _adminUserService.GetAdminByIdAsync(userId);
                if (admin == null)
                    return NotFound(new { success = false, message = "Admin user not found" });

                return Ok(new { success = true, data = admin });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error retrieving admin user: {UserId}", userId);
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving admin user" });
            }
        }

        /// <summary>
        /// Deactivate admin user
        /// </summary>
        [HttpPost("{userId}/deactivate")]
        public async Task<IActionResult> DeactivateAdmin(string userId)
        {
            try
            {
                var deactivatedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(deactivatedBy))
                    return Unauthorized(new { success = false, message = "Admin ID not found" });

                if (userId == deactivatedBy)
                    return BadRequest(new { success = false, message = "Cannot deactivate your own account" });

                var success = await _adminUserService.DeactivateAdminAsync(userId);
                if (!success)
                    return NotFound(new { success = false, message = "Admin user not found" });

                _logger.Information("Admin user deactivated by {DeactivatedBy}: {UserId}", deactivatedBy, userId);
                return Ok(new { success = true, message = "Admin user deactivated successfully" });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error deactivating admin user: {UserId}", userId);
                return StatusCode(500, new { success = false, message = "An error occurred while deactivating admin user" });
            }
        }

        /// <summary>
        /// Reactivate admin user
        /// </summary>
        [HttpPost("{userId}/reactivate")]
        public async Task<IActionResult> ReactivateAdmin(string userId)
        {
            try
            {
                var reactivatedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(reactivatedBy))
                    return Unauthorized(new { success = false, message = "Admin ID not found" });

                var success = await _adminUserService.ReactivateAdminAsync(userId);
                if (!success)
                    return NotFound(new { success = false, message = "Admin user not found" });

                _logger.Information("Admin user reactivated by {ReactivatedBy}: {UserId}", reactivatedBy, userId);
                return Ok(new { success = true, message = "Admin user reactivated successfully" });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error reactivating admin user: {UserId}", userId);
                return StatusCode(500, new { success = false, message = "An error occurred while reactivating admin user" });
            }
        }

        /// <summary>
        /// Reset admin user password
        /// </summary>
        [HttpPost("{userId}/reset-password")]
        public async Task<IActionResult> ResetAdminPassword(string userId, [FromBody] ResetPasswordRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid input" });

            try
            {
                var resetBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(resetBy))
                    return Unauthorized(new { success = false, message = "Admin ID not found" });

                if (string.IsNullOrWhiteSpace(dto.NewPassword))
                    return BadRequest(new { success = false, message = "New password cannot be empty" });

                var success = await _adminUserService.ResetAdminPasswordAsync(userId, dto.NewPassword, resetBy);
                if (!success)
                    return BadRequest(new { success = false, message = "Failed to reset password" });

                _logger.Information("Admin password reset by {ResetBy}: {UserId}", resetBy, userId);
                return Ok(new { success = true, message = "Admin user password reset successfully" });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error resetting admin password: {UserId}", userId);
                return StatusCode(500, new { success = false, message = "An error occurred while resetting password" });
            }
        }

        /// <summary>
        /// Delete admin user
        /// </summary>
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteAdmin(string userId)
        {
            try
            {
                var deletedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(deletedBy))
                    return Unauthorized(new { success = false, message = "Admin ID not found" });

                if (userId == deletedBy)
                    return BadRequest(new { success = false, message = "Cannot delete your own account" });

                var success = await _adminUserService.DeleteAdminAsync(userId, deletedBy);
                if (!success)
                    return NotFound(new { success = false, message = "Admin user not found" });

                _logger.Information("Admin user deleted by {DeletedBy}: {UserId}", deletedBy, userId);
                return Ok(new { success = true, message = "Admin user deleted successfully" });
            }
            catch (ArgumentException ex)
            {
                _logger.Warning(ex, "Invalid argument: {Message}", ex.Message);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error deleting admin user: {UserId}", userId);
                return StatusCode(500, new { success = false, message = "An error occurred while deleting admin user" });
            }
        }

        /// <summary>
        /// Check if admin user is active
        /// </summary>
        [HttpGet("{userId}/status")]
        public async Task<IActionResult> GetAdminStatus(string userId)
        {
            try
            {
                var isActive = await _adminUserService.IsAdminActiveAsync(userId);
                return Ok(new { success = true, isActive });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error checking admin status: {UserId}", userId);
                return StatusCode(500, new { success = false, message = "An error occurred while checking admin status" });
            }
        }
    }
}
