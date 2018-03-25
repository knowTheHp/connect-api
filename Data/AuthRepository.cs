using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ConnectApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ConnectApi.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _context;
        public AuthRepository(DataContext context) => this._context = context;
        public async Task<bool> UsernameExists(string username)
        {
            if (await _context.User.AnyAsync(x => x.Username == username)) return true;
            return false;
        }
        public async Task<bool> EmailExists(string email)
        {
            if (await _context.User.AnyAsync(x => x.Email == email)) return true;
            return false;
        }
        public async Task<User> Register(User user, string password)
        {
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);
            user.PasswordHash = passwordHash;
            user.passwordSalt = passwordSalt;
            await _context.User.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> Login(string email, string password)
        {
            var user = await _context.User.Include(p => p.Photos).FirstOrDefaultAsync(x => x.Email == email);
            if (user == null) return null;
            if (!VerifyPassword(password, user.PasswordHash, user.passwordSalt)) return null;
            return user;
        }

        void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPassword(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != passwordHash[i]) return false;
                }
            }
            return true;
        }
    }
}