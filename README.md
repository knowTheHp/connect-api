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
    
    
## Code
    
**Auth Controller**
<br/>
Auth Controller takes care of user registration and login. A repository is created [IAuthRepository](https://github.com/knowTheHp/connect-api/blob/master/Data/IAuthRepository.cs) that takes care of storing the user information in the database and validates the user for the same.
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

**IAuth Repo**
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

**User Controller**
<br/>
User Controller is all about handling user requests. The request could be to display a list of other users and their profiles or to update user details. A repository [IConnectRepository](https://github.com/knowTheHp/connect-api/blob/master/Data/IAuthRepository.cs) is created that takes care of handling the requests.
```c#
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using connect_api.Data;
using connect_api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace connect_api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly IConnectRepository _connectRepository;
        private readonly IMapper _mapper;
        public UserController(IConnectRepository connectRepository, IMapper mapper)
        {
            this._mapper = mapper;
            this._connectRepository = connectRepository;
        }

        //get users
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await this._connectRepository.GetUsers();
            var returnUsers = this._mapper.Map<IEnumerable<UserListDto>>(users);
            return Ok(returnUsers);
        }

        //get user
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await this._connectRepository.GetUser(id);
            var returnUser = this._mapper.Map<UserDetailDto>(user);
            return Ok(returnUser);
        }

        //update user
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDto userUpdateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //get current user
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            //get user from repo
            var user = await this._connectRepository.GetUser(id);

            if (user == null)
                return NotFound($"could not find user with an id of {id}");

            if (currentUserId != user.Id)
                return Unauthorized();

            this._mapper.Map(userUpdateDto, user);
            if (await this._connectRepository.SaveAll())
                return NoContent();

            throw new Exception($"Upating user {id} failed on save");
        }
    }
}
```
**IConnect Repo**
```c#
using System.Collections.Generic;
using System.Threading.Tasks;
using ConnectApi.Models;

namespace connect_api.Data
{
    public interface IConnectRepository
    {
        void Add<T>(T entity) where T : class;
        void Delete<T>(T entity) where T : class;
        Task<bool> SaveAll();

        Task<IEnumerable<User>> GetUsers();
        Task<User> GetUser(int id);
    }
}
```

**Connect Repo**
```c#

using System.Collections.Generic;
using System.Threading.Tasks;
using ConnectApi.Data;
using ConnectApi.Models;
using Microsoft.EntityFrameworkCore;

namespace connect_api.Data
{
    public class ConnectRepository : IConnectRepository
    {
        private readonly DataContext _context;
        public ConnectRepository(DataContext context) => this._context = context;
        public void Add<T>(T entity) where T : class => _context.Add(entity);
        public void Delete<T>(T entity) where T : class => _context.Remove(entity);
        public async Task<User> GetUser(int id)
        {
            var user = await this._context.User.Include(p => p.Photos)
            .Include(e => e.Education).Include(proj => proj.Projects)
            .Include(s => s.Skills).Include(w => w.WorkExperiences)
            .FirstOrDefaultAsync(u => u.Id == id);

            return user;
        }
        public async Task<IEnumerable<User>> GetUsers()
        {
            var users = await this._context.User.Include(p => p.Photos)
            .Include(s => s.Skills)
            .ToListAsync();

            return users;
        }
        public async Task<bool> SaveAll() => await this._context.SaveChangesAsync() > 0;
    }
}
```
