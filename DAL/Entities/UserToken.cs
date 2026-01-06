using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class UserToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; } = null!;
        public string TokenType { get; set; } = null!; // "EmailConfirmation" or "PasswordReset"
        public DateTime Expiration { get; set; }
        public bool IsUsed { get; set; } = false;
    }

}
