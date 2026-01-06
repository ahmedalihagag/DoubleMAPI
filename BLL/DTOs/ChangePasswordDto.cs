using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs
{
    public class ForgotPasswordDto
    {
        public string Email { get; set; } = null!;
    }

    public class ResetPasswordDto
    {
        public string UserId { get; set; } = null!;
        public string Token { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
}
