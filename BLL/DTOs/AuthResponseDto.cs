using BLL.DTOs.UserDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs
{
    public class AuthResponseDto
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }

        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }

        public UserDto? User { get; set; }
    }

}
