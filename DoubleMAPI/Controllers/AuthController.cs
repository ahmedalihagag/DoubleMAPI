using BLL.DTOs;
using BLL.DTOs.UserDTOs;
using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Threading.Tasks;

namespace DoubleMAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly Serilog.ILogger _logger;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
            _logger = Log.ForContext<AuthController>();
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid input", errors = ModelState.Values });

            try
            {
                var result = await _authService.RegisterUserAsync(dto);
                if (!result.IsSuccess)
                    return BadRequest(new { success = false, message = result.Message });

                return Ok(new { success = true, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error registering user");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Login user
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid input" });

            try
            {
                var result = await _authService.LoginAsync(dto);
                if (!result.IsSuccess)
                    return Unauthorized(new { success = false, message = result.Message });

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error logging in user");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Refresh JWT token
        /// </summary>
        [HttpPost("refresh-token")]
        public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid input" });

            try
            {
                var result = await _authService.RefreshTokenAsync(dto);
                if (!result.IsSuccess)
                    return Unauthorized(new { success = false, message = result.Message });

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error refreshing token");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Logout user
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "User ID not found" });

                await _authService.LogoutAsync(userId);
                return Ok(new { success = true, message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error logging out");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Confirm email (from registration link)
        /// </summary>
        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            try
            {
                var success = await _authService.ConfirmEmailAsync(userId, token);
                if (!success)
                    return BadRequest(new { success = false, message = "Email confirmation failed" });

                return Ok(new { success = true, message = "Email confirmed successfully" });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error confirming email");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Change password
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] string user_Id, [FromBody]string currentPassword,[FromBody] string newPassword)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid input" });

            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "User ID not found" });

                var success = await _authService.ChangePasswordAsync(userId, currentPassword, newPassword);
                if (!success)
                    return BadRequest(new { success = false, message = "Password change failed" });

                return Ok(new { success = true, message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error changing password");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Forgot password
        /// </summary>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid input" });

            try
            {
                await _authService.SendPasswordResetAsync(dto);
                // Don't reveal if email exists
                return Ok(new { success = true, message = "If email exists, password reset link sent" });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error sending password reset");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Reset password
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid input" });

            try
            {
                var success = await _authService.ResetPasswordAsync(dto);
                if (!success)
                    return BadRequest(new { success = false, message = "Password reset failed" });

                return Ok(new { success = true, message = "Password reset successfully" });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error resetting password");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }
    }
}