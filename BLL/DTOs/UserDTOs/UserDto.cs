using System;
using System.Collections.Generic;

namespace BLL.DTOs.UserDTOs
{
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public string? PhotoUrl { get; set; }

        public string? Specialty { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<string> Roles { get; set; } = new();
    }
}
