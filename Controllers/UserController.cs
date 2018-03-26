using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using connect_api.Data;
using connect_api.Dtos;
using connect_api.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace connect_api.Controllers
{
    [ServiceFilter(typeof(UserActivity))]
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
        [HttpGet("{id}", Name = "GetUser")]
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