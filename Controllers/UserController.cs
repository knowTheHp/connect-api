using System.Collections.Generic;
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
    }
}