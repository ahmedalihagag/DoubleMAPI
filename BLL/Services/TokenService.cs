using BLL.DTOs.UserDTOs;
using BLL.Interfaces;
using BLL.Settings;
using DAL.Entities;
using DAL.Enums;
using DAL.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwt;
        private readonly IUnitOfWork _uow;

        public TokenService(IOptions<JwtSettings> jwt, IUnitOfWork uow)
        {
            _jwt = jwt.Value;
            _uow = uow;
        }

        // -------------------------
        // JWT
        // -------------------------
        public async Task<string> GenerateJwtTokenAsync(UserDto user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.FullName)
            }.ToList();

            if (user.Roles != null)
                claims.AddRange(user.Roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // -------------------------
        // Refresh Tokens
        // -------------------------
        public async Task<string> GenerateRefreshTokenAsync(string userId)
        {
            var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var tokenEntity = new RefreshToken
            {
                UserId = userId,
                TokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken))),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30), // Example 30 days
                IsRevoked = false
            };

            await _uow.RefreshTokens.AddAsync(tokenEntity);
            await _uow.SaveChangesAsync();

            return rawToken;
        }

        public async Task<string> RotateRefreshTokenAsync(string oldToken, string userId)
        {
            var oldHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(oldToken)));
            var tokenEntity = await _uow.RefreshTokens.GetByTokenHashAsync(oldHash);
            if (tokenEntity == null || tokenEntity.IsRevoked)
                throw new Exception("Invalid refresh token");

            tokenEntity.IsRevoked = true;
            tokenEntity.RevokedAt = DateTime.UtcNow;

            var newRaw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var newTokenEntity = new RefreshToken
            {
                UserId = userId,
                TokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(newRaw))),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                IsRevoked = false
            };

            await _uow.RefreshTokens.AddAsync(newTokenEntity);
            await _uow.SaveChangesAsync();

            return newRaw;
        }

        public async Task<bool> ValidateRefreshTokenAsync(string refreshToken, string userId)
        {
            var hash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
            var tokenEntity = await _uow.RefreshTokens.GetByTokenHashAsync(hash);
            return tokenEntity != null && tokenEntity.UserId == userId && !tokenEntity.IsRevoked && tokenEntity.ExpiresAt > DateTime.UtcNow;
        }

        public async Task RevokeRefreshTokensAsync(string userId)
        {
            await _uow.RefreshTokens.RevokeAllUserTokensAsync(userId);
            await _uow.SaveChangesAsync();
        }

        // -------------------------
        // Expired JWT Principal
        // -------------------------
        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = false,
                ValidIssuer = _jwt.Issuer,
                ValidAudience = _jwt.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey))
            };

            var handler = new JwtSecurityTokenHandler();
            try
            {
                var principal = handler.ValidateToken(token, validationParameters, out var securityToken);
                if (securityToken is JwtSecurityToken jwt &&
                    jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
                {
                    return principal;
                }
            }
            catch { }

            return null;
        }
    }
}