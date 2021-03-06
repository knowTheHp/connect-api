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

**User Model**
<br/>
```c#
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace ConnectApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Username { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] passwordSalt { get; set; }
        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastActive { get; set; }
        public string Introduction { get; set; }
        public string Interests { get; set; }
        public string City { get; set; }
        public string Country { get; set; }

        //1 to many
        public ICollection<Education> Education { get; set; }
        public ICollection<WorkExperience> WorkExperiences { get; set; }
        public ICollection<Skill> Skills { get; set; }
        public ICollection<Project> Projects { get; set; }
        public ICollection<Photo> Photos { get; set; }

        public User()
        {

        }
    }
}
```
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

**AutoMapper**
<br/>
```c#
using System.Linq;
using AutoMapper;
using connect_api.Dtos;
using ConnectApi.Helpers;
using ConnectApi.Models;

namespace connect_api.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<User, UserListDto>()
            .ForMember(destination => destination.PhotoUrl, option => option.MapFrom(src => src.Photos.FirstOrDefault(photo => photo.IsMain).Url))
            .ForMember(destination => destination.Age, option => option.ResolveUsing(d => d.DateOfBirth.CalculateAge()));

            CreateMap<User, UserDetailDto>()
            .ForMember(destination => destination.PhotoUrl, option => option.MapFrom(src => src.Photos.FirstOrDefault(photo => photo.IsMain).Url))
            .ForMember(destination => destination.Age, option => option.ResolveUsing(d => d.DateOfBirth.CalculateAge()));

            CreateMap<Education, EducationDetailDto>();
            CreateMap<WorkExperience, WorkExperienceDetailDto>();
            CreateMap<Skill, SkillDetailDto>();
            CreateMap<Project, ProjectDetailDto>();
            CreateMap<Photo, PhotoDetailDto>();

            //update dto
            CreateMap<UserUpdateDto, User>();
        }
    }
}
```

**Photo Controller**
<br/>
Photo Controller takes care of photo upload and save it to cloudinary. It stores the public id and path of the image in db.
```c#
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using connect_api.Data;
using connect_api.Dtos;
using connect_api.Helpers;
using ConnectApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace connect_api.Controllers
{
    [Authorize]
    [Route("api/users/{userId}/photos")]
    public class PhotosController : Controller
    {
        private readonly IConnectRepository _connectRepository;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _cloudinaryOptions;
        private Cloudinary _cloudinary;

        public PhotosController(IConnectRepository connectRepository, IMapper mapper, IOptions<CloudinarySettings> cloudinaryOptions)
        {
            this._cloudinaryOptions = cloudinaryOptions;
            this._mapper = mapper;
            this._connectRepository = connectRepository;

            Account account = new Account(this._cloudinaryOptions.Value.CloudName, this._cloudinaryOptions.Value.ApiKey, this._cloudinaryOptions.Value.ApiSecret);

            this._cloudinary = new Cloudinary(account);
        }

        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photo = await this._connectRepository.GetPhoto(id);
            var PhotoFetchDto = this._mapper.Map<PhotoFetchDto>(photo);
            return Ok(PhotoFetchDto);

        }

        //add photos
        [HttpPost]
        public async Task<IActionResult> AddPhoto(int userId, PhotoCreationDto photoDto)
        {
            User user = await this._connectRepository.GetUser(userId);
            if (user == null)
                return BadRequest("could not find user");

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (currentUserId != user.Id)
                return Unauthorized();

            IFormFile file = photoDto.File;
            ImageUploadResult uploadResult = new ImageUploadResult();
            if (file.Length > 0)
            {
                using (Stream stream = file.OpenReadStream())
                {
                    ImageUploadParams uploadPrams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                    };
                    uploadResult = this._cloudinary.Upload(uploadPrams);
                }
            }
            photoDto.Url = uploadResult.Uri.ToString();
            photoDto.PublicId = uploadResult.PublicId;

            Photo photo = this._mapper.Map<Photo>(photoDto);
            photo.User = user;

            if (!user.Photos.Any(m => m.IsMain))
                photo.IsMain = true;

            user.Photos.Add(photo);
            var returnPhoto = this._mapper.Map<PhotoFetchDto>(photo);
            if (await this._connectRepository.SaveAll())
            {
                return CreatedAtRoute("GetPhoto", new { id = photo.Id }, returnPhoto);
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
```
**Data Context**
<br/>
This application uses **Entity Framework** for database connectivity. It requires a DbContext object for the same.
```c#
using ConnectApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ConnectApi.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }
        public DbSet<Value> Values { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<Education> Education { get; set; }
        public DbSet<Photo> Photo { get; set; }
        public DbSet<Project> Project { get; set; }
        public DbSet<Skill> Skill { get; set; }
        public DbSet<WorkExperience> WorkExperience { get; set; }
    }
}
```
**appSettings.json**
<br/>

The file contains json token key, connection string, and cloudinary configuration 
```javascript
{
  "AppSettings": {
    "Token": "this is a super secret key"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database = ypour;Trusted_Connection = True;"
  },
  "Logging": {
    "IncludeScopes": false,
    "Debug": {
      "LogLevel": {
        "Default": "Warning"
      }
    },
    "Console": {
      "LogLevel": {
        "Default": "Warning"
      }
    },
    "CloudinarySettings": {
      "CloudName": "your cloud name",
      "ApiKey": "your api key",
      "ApiSecret": "your secret key"
    }
  }
}
```

**Startup.cs**
<br/>

```c#
using System.Net;
using System.Text;
using AutoMapper;
using connect_api.Data;
using connect_api.Helpers;
using ConnectApi.Data;
using ConnectApi.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace ConnectApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var key = Encoding.ASCII.GetBytes(Configuration.GetSection("AppSettings:Token").Value);
            services.AddDbContext<DataContext>(x => x.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddTransient<Seed>();
            services.AddAutoMapper();
            services.AddMvc();
            services.AddCors();
            services.Configure<CloudinarySettings>(Configuration.GetSection("CloudinarySettings"));
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IConnectRepository, ConnectRepository>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
            services.AddMvc().AddJsonOptions(opt =>
            {
                opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, Seed seed)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                //global exception handler
                app.UseExceptionHandler(builder =>
                {
                    builder.Run(async context =>
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        var error = context.Features.Get<IExceptionHandlerFeature>();
                        if (error != null)
                        {
                            context.Response.AddApplicationError(error.Error.Message);
                            await context.Response.WriteAsync(error.Error.Message).ConfigureAwait(false);
                        }
                    });
                });
            }
            //seed.SeedUser();
            app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().AllowCredentials());
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}

```
**Connect-api.csproj**
<br/>
This file contains all the packages used during the development of the project.
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>netcoreapp2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <Folder Include="wwwroot\"/>
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.All" Version="2.0.5"/>
    <PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="3.1.0"/>
    <PackageReference Include="CloudinaryDotNet" Version="1.1.1"/>
  </ItemGroup>
  <ItemGroup>
    <DotNetCliToolReference Include="Microsoft.VisualStudio.Web.CodeGeneration.Tools" Version="2.0.2"/>
    <DotNetCliToolReference Include="Microsoft.DotNet.Watcher.Tools" Version="2.0.0"/>
    <DotNetCliToolReference Include="Microsoft.EntityFrameworkCore.Tools.DotNet" Version="2.0.0"/>
  </ItemGroup>
</Project>
```
