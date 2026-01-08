using DAL.Data;
using DAL.Entities;
using DAL.Enums;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class UserTokenRepository: Repository<UserToken>,IUserTokenRepository
    {
        private readonly ApplicationDbContext _context;

        public UserTokenRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<UserToken?> GetValidTokenAsync(string token)
        {
            return await _context.UserTokens
        .FirstOrDefaultAsync(t =>
            t.Token == token &&
            t.TokenType == UserTokenType.Biometric &&
            !t.IsUsed);
        }

    }
}
