using BLL.DTOs.UserDTOs;
using DAL.Entities;
using DAL.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface ITokenService
    {
        Task<string> GenerateJwtTokenAsync(UserDto user);
        Task<string> GenerateRefreshTokenAsync(string userId);
        Task<string> RotateRefreshTokenAsync(string oldToken, string userId);
        Task<bool> ValidateRefreshTokenAsync(string refreshToken, string userId);
        Task RevokeRefreshTokensAsync(string userId);
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
