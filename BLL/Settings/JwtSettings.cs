using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Settings
{
    public class JwtSettings
    {
        public string SecretKey { get; set; } = null!; // Minimum 16+ chars
        public string Issuer { get; set; } = null!;
        public string Audience { get; set; } = null!;
        public int ExpiryMinutes { get; set; } // e.g., 60
        public int RefreshTokenExpiryDays { get; set; }
    }
}
