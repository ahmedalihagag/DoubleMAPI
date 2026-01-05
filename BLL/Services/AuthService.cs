using AutoMapper;
using BLL.DTOs;
using BLL.DTOs.UserDTOs;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly ILogger _logger;
        private readonly JwtSettings _jwtSettings;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IEmailService emailService,
            IOptions<JwtSettings> jwtSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _emailService = emailService;
            _logger = Log.ForContext<AuthService>();
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                _logger.Information("Registering user: {Email} with role: {Role}",
                    registerDto.Email, registerDto.Role);

                var user = _mapper.Map<ApplicationUser>(registerDto);

                var result = await _userManager.CreateAsync(user, registerDto.Password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.Warning("User registration failed: {Errors}", errors);
                    throw new Exception($"Registration failed: {errors}");
                }

                await _userManager.AddToRoleAsync(user, registerDto.Role);

                // Send welcome email
                try
                {
                    await _emailService.SendWelcomeEmailAsync(user.Email, user.FullName ?? "User");
                }
                catch (Exception emailEx)
                {
                    _logger.Warning(emailEx, "Failed to send welcome email");
                }

                _logger.Information("User registered successfully: {Email}", registerDto.Email);

                return await GenerateAuthResponseAsync(user);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error registering user: {Email}", registerDto.Email);
                throw;
            }
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            try
            {
                _logger.Information("User login attempt: {Email}", loginDto.Email);

                var user = await _userManager.FindByEmailAsync(loginDto.Email);
                if (user == null)
                {
                    _logger.Warning("User not found: {Email}", loginDto.Email);
                    throw new UnauthorizedAccessException("Invalid credentials");
                }

                // Check if user is blocked
                var isBlocked = await _unitOfWork.Blacklists.IsUserBlockedAsync(user.Id);
                if (isBlocked)
                {
                    _logger.Warning("Blocked user attempted login: {Email}", loginDto.Email);
                    throw new UnauthorizedAccessException("Your account has been blocked");
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

                if (!result.Succeeded)
                {
                    _logger.Warning("Invalid password for user: {Email}", loginDto.Email);
                    throw new UnauthorizedAccessException("Invalid credentials");
                }

                _logger.Information("User logged in successfully: {Email}", loginDto.Email);

                return await GenerateAuthResponseAsync(user);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error during login: {Email}", loginDto.Email);
                throw;
            }
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
        {
            try
            {
                _logger.Debug("Refreshing token");

                var storedToken = await _unitOfWork.RefreshTokens
                    .GetByTokenHashAsync(HashToken(refreshTokenDto.RefreshToken));

                if (storedToken == null || storedToken.ExpiresAt < DateTime.UtcNow)
                {
                    _logger.Warning("Invalid or expired refresh token");
                    throw new UnauthorizedAccessException("Invalid refresh token");
                }

                var user = await _userManager.FindByIdAsync(storedToken.UserId);
                if (user == null)
                    throw new UnauthorizedAccessException("User not found");

                // Revoke old token
                storedToken.IsRevoked = true;
                storedToken.RevokedAt = DateTime.UtcNow;
                _unitOfWork.RefreshTokens.Update(storedToken);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information("Token refreshed for user: {UserId}", user.Id);

                return await GenerateAuthResponseAsync(user);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error refreshing token");
                throw;
            }
        }

        public async Task<bool> LogoutAsync(string userId)
        {
            try
            {
                _logger.Information("User logout: {UserId}", userId);

                await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(userId);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information("User logged out successfully: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error during logout: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            try
            {
                _logger.Information("Changing password for user: {UserId}", userId);

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return false;

                var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

                if (result.Succeeded)
                {
                    _logger.Information("Password changed successfully for user: {UserId}", userId);
                    return true;
                }

                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.Warning("Password change failed: {Errors}", errors);
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error changing password for user: {UserId}", userId);
                throw;
            }
        }

        private async Task<AuthResponseDto> GenerateAuthResponseAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, roles.ToList());
            var refreshToken = GenerateRefreshToken();

            // Store refresh token
            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = HashToken(refreshToken),
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
            await _unitOfWork.SaveChangesAsync();

            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = roles.ToList();

            return new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                User = userDto
            };
        }

        private string GenerateJwtToken(ApplicationUser user, List<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName ?? user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        private string HashToken(string token)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
