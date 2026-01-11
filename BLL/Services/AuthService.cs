using AutoMapper;
using BLL.DTOs;
using BLL.DTOs.UserDTOs;
using BLL.Interfaces;
using DAL.Enums;
using DAL.Interfaces;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly IDeviceSessionService _deviceSessionService;
        private readonly ILogger _logger;

        public AuthService(
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            ITokenService tokenService,
            IMapper mapper,
            IDeviceSessionService deviceSessionService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _deviceSessionService = deviceSessionService ?? throw new ArgumentNullException(nameof(deviceSessionService));
            _logger = Log.ForContext<AuthService>();
        }

        public async Task<AuthResponseDto> RegisterUserAsync(RegisterUserDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Email))
                    return new AuthResponseDto { IsSuccess = false, Message = "Email is required" };

                if (string.IsNullOrWhiteSpace(dto.Password))
                    return new AuthResponseDto { IsSuccess = false, Message = "Password is required" };

                if (string.IsNullOrWhiteSpace(dto.FullName))
                    return new AuthResponseDto { IsSuccess = false, Message = "Full name is required" };

                var existingUser = await _unitOfWork.UserManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                {
                    _logger.Warning("Registration attempt with existing email: {Email}", dto.Email);
                    return new AuthResponseDto { IsSuccess = false, Message = "Email already exists" };
                }

                var user = new DAL.Entities.ApplicationUser
                {
                    Email = dto.Email,
                    UserName = dto.Email,
                    FullName = dto.FullName,
                    PhoneNumber = dto.PhoneNumber,
                    CreatedAt = DateTime.UtcNow,
                    EmailConfirmed = false
                };

                var result = await _unitOfWork.UserManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    _logger.Error("User creation failed: {Errors}", errors);
                    return new AuthResponseDto { IsSuccess = false, Message = $"Registration failed: {errors}" };
                }

                // Normalize role
                var role = string.IsNullOrWhiteSpace(dto.Role) ? "Student" : dto.Role.Trim();

                if (!await _unitOfWork.RoleManager.RoleExistsAsync(role))
                {
                    var roleResult = await _unitOfWork.RoleManager.CreateAsync(new IdentityRole(role));
                    if (!roleResult.Succeeded)
                    {
                        _logger.Error("Failed to create role: {Role}", role);
                        return new AuthResponseDto { IsSuccess = false, Message = "Role creation failed" };
                    }
                }

                var addRoleResult = await _unitOfWork.UserManager.AddToRoleAsync(user, role);
                if (!addRoleResult.Succeeded)
                {
                    var errors = string.Join("; ", addRoleResult.Errors.Select(e => e.Description));
                    _logger.Error("Failed to assign role: {Errors}", errors);
                    return new AuthResponseDto { IsSuccess = false, Message = $"Role assignment failed: {errors}" };
                }

                // Generate email confirmation token
                var emailToken = await _unitOfWork.UserManager.GenerateEmailConfirmationTokenAsync(user);

                try
                {
                    await _emailService.SendAsync(
                        dto.Email,
                        "Confirm your email",
                        $"Welcome {dto.FullName}, please confirm your email to activate your account.");
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "Failed to send confirmation email to {Email}", dto.Email);
                }

                _logger.Information("User registered successfully: {Email} with role {Role}", dto.Email, role);

                return new AuthResponseDto
                {
                    IsSuccess = true,
                    Message = "Registration successful. Please confirm your email."
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error during user registration");
                return new AuthResponseDto { IsSuccess = false, Message = "An error occurred during registration" };
            }
        }

        public async Task<AuthResponseDto> LoginAsync(
            LoginDto dto,
            string deviceId,
            ClientType clientType,
            string deviceInfo,
            string ipAddress)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                    return new AuthResponseDto { IsSuccess = false, Message = "Invalid credentials" };

                var user = await _unitOfWork.UserManager.FindByEmailAsync(dto.Email);
                if (user == null)
                {
                    _logger.Warning("Login attempt with non-existent email: {Email}", dto.Email);
                    return new AuthResponseDto { IsSuccess = false, Message = "Invalid credentials" };
                }

                if (!await _unitOfWork.UserManager.CheckPasswordAsync(user, dto.Password))
                {
                    _logger.Warning("Login attempt with wrong password for: {Email}", dto.Email);
                    return new AuthResponseDto { IsSuccess = false, Message = "Invalid credentials" };
                }

                if (!user.EmailConfirmed)
                {
                    _logger.Warning("Login attempt with unconfirmed email: {Email}", dto.Email);
                    return new AuthResponseDto { IsSuccess = false, Message = "Email not confirmed. Please check your inbox." };
                }

                // Validate/create device session
                await _deviceSessionService.ValidateAndCreateSessionAsync(
                    user.Id,
                    deviceId,
                    clientType,
                    deviceInfo,
                    ipAddress);

                var userDto = _mapper.Map<UserDto>(user);
                userDto.Roles = (await _unitOfWork.UserManager.GetRolesAsync(user)).ToList();

                var jwtToken = await _tokenService.GenerateJwtTokenAsync(userDto);
                var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id);

                _logger.Information("User logged in successfully: {Email} via {ClientType}", dto.Email, clientType);

                return new AuthResponseDto
                {
                    IsSuccess = true,
                    Token = jwtToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddHours(1),
                    Message = "Login successful",
                    User = userDto
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error during login for email: {Email}", dto.Email);
                return new AuthResponseDto { IsSuccess = false, Message = "An error occurred during login" };
            }
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Token) || string.IsNullOrWhiteSpace(dto.RefreshToken))
                    return new AuthResponseDto { IsSuccess = false, Message = "Invalid token" };

                var principal = _tokenService.GetPrincipalFromExpiredToken(dto.Token);
                if (principal == null)
                {
                    _logger.Warning("Invalid token during refresh");
                    return new AuthResponseDto { IsSuccess = false, Message = "Invalid token" };
                }

                var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.Warning("User ID not found in token");
                    return new AuthResponseDto { IsSuccess = false, Message = "Invalid token" };
                }

                var valid = await _tokenService.ValidateRefreshTokenAsync(dto.RefreshToken, userId);
                if (!valid)
                {
                    _logger.Warning("Invalid refresh token for user: {UserId}", userId);
                    return new AuthResponseDto { IsSuccess = false, Message = "Invalid refresh token" };
                }

                var user = await _unitOfWork.UserManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.Warning("User not found during token refresh: {UserId}", userId);
                    return new AuthResponseDto { IsSuccess = false, Message = "User not found" };
                }

                var userDto = _mapper.Map<UserDto>(user);
                userDto.Roles = (await _unitOfWork.UserManager.GetRolesAsync(user)).ToList();

                var newJwt = await _tokenService.GenerateJwtTokenAsync(userDto);
                var newRefresh = await _tokenService.RotateRefreshTokenAsync(dto.RefreshToken, user.Id);

                _logger.Information("Token refreshed successfully for user: {UserId}", userId);

                return new AuthResponseDto
                {
                    IsSuccess = true,
                    Token = newJwt,
                    RefreshToken = newRefresh,
                    ExpiresAt = DateTime.UtcNow.AddHours(1),
                    Message = "Token refreshed successfully",
                    User = userDto
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error during token refresh");
                return new AuthResponseDto { IsSuccess = false, Message = "An error occurred during token refresh" };
            }
        }

        public async Task<bool> LogoutAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return false;

                await _tokenService.RevokeRefreshTokensAsync(userId);
                _logger.Information("User logged out: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error during logout for user: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return false;

                var user = await _unitOfWork.UserManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.Warning("User not found for password change: {UserId}", userId);
                    return false;
                }

                var result = await _unitOfWork.UserManager.ChangePasswordAsync(user, currentPassword, newPassword);

                if (result.Succeeded)
                {
                    _logger.Information("Password changed successfully for user: {UserId}", userId);
                }
                else
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    _logger.Warning("Password change failed for user {UserId}: {Errors}", userId, errors);
                }

                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error changing password for user: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> ConfirmEmailAsync(string userId, string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
                    return false;

                var user = await _unitOfWork.UserManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.Warning("User not found for email confirmation: {UserId}", userId);
                    return false;
                }

                var result = await _unitOfWork.UserManager.ConfirmEmailAsync(user, token);

                if (result.Succeeded)
                {
                    _logger.Information("Email confirmed successfully for user: {UserId}", userId);
                }
                else
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    _logger.Warning("Email confirmation failed for user {UserId}: {Errors}", userId, errors);
                }

                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error confirming email for user: {UserId}", userId);
                return false;
            }
        }

        public async Task SendPasswordResetAsync(ForgotPasswordDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Email))
                    return;

                var user = await _unitOfWork.UserManager.FindByEmailAsync(dto.Email);
                if (user == null)
                {
                    _logger.Information("Password reset requested for non-existent email: {Email}", dto.Email);
                    return;
                }

                var token = await _unitOfWork.UserManager.GeneratePasswordResetTokenAsync(user);

                try
                {
                    await _emailService.SendAsync(
                        dto.Email,
                        "Reset Your Password",
                        $"Click the link below to reset your password. Token: {token}");

                    _logger.Information("Password reset email sent to: {Email}", dto.Email);
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "Failed to send password reset email to: {Email}", dto.Email);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error during password reset request for email: {Email}", dto.Email);
            }
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.UserId) || string.IsNullOrWhiteSpace(dto.Token) || string.IsNullOrWhiteSpace(dto.NewPassword))
                    return false;

                var user = await _unitOfWork.UserManager.FindByIdAsync(dto.UserId);
                if (user == null)
                {
                    _logger.Warning("User not found for password reset: {UserId}", dto.UserId);
                    return false;
                }

                var result = await _unitOfWork.UserManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);

                if (result.Succeeded)
                {
                    _logger.Information("Password reset successfully for user: {UserId}", dto.UserId);
                }
                else
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    _logger.Warning("Password reset failed for user {UserId}: {Errors}", dto.UserId, errors);
                }

                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error resetting password for user: {UserId}", dto.UserId);
                return false;
            }
        }

        public async Task<AuthResponseDto> BiometricLoginAsync(BiometricLoginDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.BiometricToken) || string.IsNullOrWhiteSpace(dto.DeviceId))
                    return new AuthResponseDto { IsSuccess = false, Message = "Invalid biometric credentials" };

                var storedToken = await _unitOfWork.UserTokens.GetValidTokenAsync(dto.BiometricToken);

                if (storedToken == null || storedToken.Expiration < DateTime.UtcNow)
                {
                    _logger.Warning("Invalid or expired biometric token");
                    return new AuthResponseDto { IsSuccess = false, Message = "Invalid or expired biometric token" };
                }

                var session = await _deviceSessionService.GetActiveSessionAsync(
                    storedToken.UserId.ToString(),
                    dto.ClientType,
                    dto.DeviceId);

                if (session == null)
                {
                    _logger.Warning("Device not registered for biometric login: {DeviceId}", dto.DeviceId);
                    return new AuthResponseDto { IsSuccess = false, Message = "Device not registered" };
                }

                var user = await _unitOfWork.UserManager.FindByIdAsync(storedToken.UserId.ToString());
                if (user == null)
                {
                    _logger.Warning("User not found for biometric login");
                    return new AuthResponseDto { IsSuccess = false, Message = "User not found" };
                }

                storedToken.IsUsed = true;
                storedToken.UsedAt = DateTime.UtcNow;
                _unitOfWork.UserTokens.Update(storedToken);
                await _unitOfWork.SaveChangesAsync();

                var userDto = _mapper.Map<UserDto>(user);
                userDto.Roles = (await _unitOfWork.UserManager.GetRolesAsync(user)).ToList();

                var jwtToken = await _tokenService.GenerateJwtTokenAsync(userDto);
                var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id);

                _logger.Information("User logged in via biometric: {UserId}", user.Id);

                return new AuthResponseDto
                {
                    IsSuccess = true,
                    Token = jwtToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddHours(1),
                    User = userDto
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error during biometric login");
                return new AuthResponseDto { IsSuccess = false, Message = "An error occurred during biometric login" };
            }
        }
    }
}
