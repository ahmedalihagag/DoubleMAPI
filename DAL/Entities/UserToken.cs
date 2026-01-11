using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Enums;

namespace DAL.Entities
{
    public class UserToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; } = null!;
        public UserTokenType TokenType { get; set; }
        public DateTime Expiration { get; set; }
        public bool IsUsed { get; set; } = false;
        public DateTime? UsedAt { get; set; } // Add this property to fix CS1061
    }

}
