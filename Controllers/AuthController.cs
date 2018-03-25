using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using connect_api.Dtos;
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
        private readonly IMapper _mapper;
        public AuthController(IAuthRepository repo, IConfiguration config, IMapper mapper)
        {
            this._authRepository = repo;
            this._config = config;
            this._mapper = mapper;
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
            var userDto = this._mapper.Map<UserListDto>(user);
            return Ok(new { tokenString, userDto });
        }
    }
}