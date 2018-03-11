# Connect API - (In Development)
This **API** is made with asp.net core for the Connect Angular Application.

## structure of the project
1. [Controllers](https://github.com/knowTheHp/connect-api/tree/master/Controllers)
    * Auth
    * User
    * Education
    * Work Experience
    * Skills
    * Project
    * Photos
    
2. [Repositories(Data)](https://github.com/knowTheHp/connect-api/tree/master/Data)
    * Auth
    * Connect
    * Data Context
    * IAuth
    * IConnect


3. [Data Transfer Object(DTO)](https://github.com/knowTheHp/connect-api/tree/master/Dtos)
    * Education Details
    * Photo Creation
    * Photo Details
    * Project Details
    * Skill Details
    * User Details
    * User
    * User List
    * User Update
    * Work Experience

4. [Helpers](https://github.com/knowTheHp/connect-api/tree/master/Helpers)
    * Auto Mapper Profiles
    * Cloudinary
    * Helper Extensions
    
5. [Models](https://github.com/knowTheHp/connect-api/tree/master/Models)
    * Education
    * Photo
    * Project
    * Skill
    * User
    * Work Experience
    
    
**Auth Controller**
Auth Controller takes care of user registration and login. A repository is added i.e. [IAuthRepository](https://github.com/knowTheHp/connect-api/blob/master/Data/IAuthRepository.cs) that stores the user information in the database and validates the user for the same.

```c#
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ConnectApi.Data;
using ConnectApi.Dtos;
using ConnectApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ConnectApi.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _config;
        public AuthController(IAuthRepository repo, IConfiguration config)
        {
            this._authRepository = repo;
            this._config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserDto userDto)
        {
            if (!string.IsNullOrEmpty(userDto.Username))
                userDto.Username = userDto.Username.ToLower();

            if (await _authRepository.UsernameExists(userDto.Username))
                ModelState.AddModelError("Username", "Username already exists");

            if (await _authRepository.EmailExists(userDto.Email))
                ModelState.AddModelError("Email", "email address already exists");

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var newUserToAdd = new User
            {
                Firstname = userDto.Firstname,
                Lastname = userDto.Lastname,
                Username = userDto.Username,
                Email = userDto.Email
            };
            var createUser = await _authRepository.Register(newUserToAdd, userDto.Password);
            return StatusCode(201);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto userLoginDto)
        {
            var user = await _authRepository.Login(userLoginDto.Email, userLoginDto.Password);
            if (user == null) return Unauthorized();

            //JWT Token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config.GetSection("AppSettings:Token").Value);
            var tokenDescription = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]{
                    new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                    new Claim(ClaimTypes.Name,user.Firstname)
                }),
                Expires = DateTime.Now.AddDays(5),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescription);
            var tokenString = tokenHandler.WriteToken(token);
            return Ok(new { tokenString });
        }
    }
}
````

**IAUth Repo**

```c#
using System.Threading.Tasks;
using ConnectApi.Models;

namespace ConnectApi.Data
{
    public interface IAuthRepository
    {
        Task<User> Register(User user, string password);
        Task<User> Login(string email, string password);
        Task<bool> UsernameExists(string username);
        Task<bool> EmailExists(string email);
    }
}
```
**Auth Repo**
```c#
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
            var user = await _context.User.FirstOrDefaultAsync(x => x.Email == email);
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
```
