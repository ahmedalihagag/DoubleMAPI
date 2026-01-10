using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs.UserDTOs
{
    public class UpdateAdminUserDto
    {
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }

        [MinLength(3, ErrorMessage = "Full name must be at least 3 characters")]
        public string? FullName { get; set; }

        [Phone(ErrorMessage = "Invalid phone format")]
        public string? PhoneNumber { get; set; }

        public bool? IsActive { get; set; }
    }
}
