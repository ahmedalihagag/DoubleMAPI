using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs.UserDTOs
{
    public class ResetPasswordRequestDto
    {
        [Required(ErrorMessage = "New password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
