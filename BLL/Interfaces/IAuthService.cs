using BLL.DTOs;
using BLL.DTOs.UserDTOs;
using DAL.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterUserAsync(RegisterUserDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto, string deviceId, ClientType clientType, string deviceInfo, string ipAddress);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto);
        Task<bool> LogoutAsync(string userId);
        Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        Task<bool> ConfirmEmailAsync(string userId, string token);
        Task SendPasswordResetAsync(ForgotPasswordDto dto);
        Task<bool> ResetPasswordAsync(ResetPasswordDto dto);
        Task<AuthResponseDto> BiometricLoginAsync(BiometricLoginDto dto);

    }
}
