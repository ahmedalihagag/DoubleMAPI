using AutoMapper;
using BLL.DTOs;
using BLL.DTOs.UserDTOs;
using BLL.Interfaces;
using DAL.Enums;
using DAL.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _uow;
        private readonly IEmailService _emailService;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly IDeviceSessionService _deviceSessionService;

        public AuthService(
            IUnitOfWork uow,
            IEmailService emailService,
            ITokenService tokenService,
            IMapper mapper)
        {
            _uow = uow;
            _emailService = emailService;
            _tokenService = tokenService;
            _mapper = mapper;
        }

        // -------------------------
        // Register
        // -------------------------
        public async Task<AuthResponseDto> RegisterUserAsync(RegisterUserDto dto)
        {
            // Check if user exists
            var existingUser = await _uow.UserManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return new AuthResponseDto { IsSuccess = false, Message = "Email already exists" };

            // Create new user
            var user = new DAL.Entities.ApplicationUser
            {
                Email = dto.Email,
                UserName = dto.Email,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _uow.UserManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return new AuthResponseDto { IsSuccess = false, Message = string.Join("; ", result.Errors) };

            // Ensure role exists
            if (!await _uow.RoleManager.RoleExistsAsync(dto.Role))
                await _uow.RoleManager.CreateAsync(new IdentityRole(dto.Role));

            await _uow.UserManager.AddToRoleAsync(user, dto.Role);

            // Send confirmation email
            var emailToken = await _uow.UserManager.GenerateEmailConfirmationTokenAsync(user);
            await _emailService.SendAsync(dto.Email, "Confirm your email",
                $"Welcome {dto.FullName}, click <a href='https://yourdomain.com/account/confirm-email?token={emailToken}'>here</a> to confirm your email.");

            return new AuthResponseDto
            {
                IsSuccess = true,
                Message = "Registration successful. Please confirm your email."
            };
        }


        // -------------------------
        // Login
        // -------------------------
        public async Task<AuthResponseDto> LoginAsync(
        LoginDto dto,
        string deviceId,
        ClientType clientType,
        string deviceInfo,
        string ipAddress)
        {
            // 1. Find user
            var user = await _uow.UserManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return new AuthResponseDto { IsSuccess = false, Message = "Invalid credentials" };

            // 2. Check password
            if (!await _uow.UserManager.CheckPasswordAsync(user, dto.Password))
                return new AuthResponseDto { IsSuccess = false, Message = "Invalid credentials" };

            // 3. Check email confirmation
            if (!user.EmailConfirmed)
                return new AuthResponseDto { IsSuccess = false, Message = "Email not confirmed" };

            // 4. Check device session (prevent multi-login if needed)
            var existingSession = await _deviceSessionService.GetActiveSessionAsync(user.Id, clientType);

            if (existingSession != null && existingSession.DeviceId != deviceId)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Account already logged in on another device. Contact admin to reset."
                };
            }

            // 5. Create / validate session
            await _deviceSessionService.ValidateAndCreateSessionAsync(user.Id, deviceId, clientType, deviceInfo, ipAddress);

            // 6. Map to DTO
            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = (await _uow.UserManager.GetRolesAsync(user)).ToList();

            // 7. Generate tokens
            var jwtToken = await _tokenService.GenerateJwtTokenAsync(userDto);
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id);

            return new AuthResponseDto
            {
                IsSuccess = true,
                Token = jwtToken,
                RefreshToken = refreshToken,
                Message = "Login successful",
                User = userDto
            };
        }

        // -------------------------
        // Refresh Token
        // -------------------------
        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto)
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(dto.Token);
            if (principal == null)
                return new AuthResponseDto { IsSuccess = false, Message = "Invalid token" };

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return new AuthResponseDto { IsSuccess = false, Message = "Invalid token" };

            var valid = await _tokenService.ValidateRefreshTokenAsync(dto.RefreshToken, userId);
            if (!valid)
                return new AuthResponseDto { IsSuccess = false, Message = "Invalid refresh token" };

            var user = await _uow.UserManager.FindByIdAsync(userId);
            if (user == null)
                return new AuthResponseDto { IsSuccess = false, Message = "User not found" };

            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = (await _uow.UserManager.GetRolesAsync(user)).ToList();

            var newJwt = await _tokenService.GenerateJwtTokenAsync(userDto);
            var newRefresh = await _tokenService.RotateRefreshTokenAsync(dto.RefreshToken, user.Id);

            return new AuthResponseDto
            {
                IsSuccess = true,
                Token = newJwt,
                RefreshToken = newRefresh,
                Message = "Token refreshed successfully",
                User = userDto
            };
        }


        // -------------------------
        // Logout
        // -------------------------
        public async Task<bool> LogoutAsync(string userId)
        {
            await _tokenService.RevokeRefreshTokensAsync(userId);
            return true;
        }

        // -------------------------
        // Change Password
        // -------------------------
        public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await _uow.UserManager.FindByIdAsync(userId);
            if (user == null) return false;

            var result = await _uow.UserManager.ChangePasswordAsync(user, currentPassword, newPassword);
            return result.Succeeded;
        }

        // -------------------------
        // Email Confirmation
        // -------------------------
        public async Task<bool> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _uow.UserManager.FindByIdAsync(userId);
            if (user == null) return false;

            var result = await _uow.UserManager.ConfirmEmailAsync(user, token);
            return result.Succeeded;
        }

        // -------------------------
        // Forgot / Reset Password
        // -------------------------
        public async Task SendPasswordResetAsync(ForgotPasswordDto dto)
        {
            var user = await _uow.UserManager.FindByEmailAsync(dto.Email);
            if (user == null) return;

            var token = await _uow.UserManager.GeneratePasswordResetTokenAsync(user);
            await _emailService.SendAsync(dto.Email, "Reset Password",
                $"Click <a href='https://yourdomain.com/account/reset-password?token={token}'>here</a> to reset your password.");
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _uow.UserManager.FindByIdAsync(dto.UserId);
            if (user == null) return false;

            var result = await _uow.UserManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
            return result.Succeeded;
        }

        //Biometric Login
        public async Task<AuthResponseDto> BiometricLoginAsync(BiometricLoginDto dto)
        {
            // 1. Validate biometric token
            var storedToken = await _uow.UserTokens.GetValidTokenAsync(dto.BiometricToken);

            if (storedToken == null || storedToken.Expiration < DateTime.UtcNow)
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Invalid or expired biometric token"
                };

            // 2. Check device session
            var session = await _deviceSessionService.GetActiveSessionAsync(
                storedToken.UserId,
                dto.ClientType,
                dto.DeviceId);

            if (session == null)
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Device not registered"
                };

            // 3. Load user
            var user = await _uow.UserManager.FindByIdAsync(storedToken.UserId.ToString());
            if (user == null)
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "User not found"
                };

            // 4. Mark token as used
            storedToken.IsUsed = true;
            await _uow.SaveChangesAsync();

            // 5. Generate tokens
            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = (await _uow.UserManager.GetRolesAsync(user)).ToList();

            var jwtToken = await _tokenService.GenerateJwtTokenAsync(userDto);
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id);

            return new AuthResponseDto
            {
                IsSuccess = true,
                Token = jwtToken,
                RefreshToken = refreshToken,
                User = userDto
            };
        }

    }
}
