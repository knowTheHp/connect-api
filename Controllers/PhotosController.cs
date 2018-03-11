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
        public async Task<IActionResult> GetPhotos(int id)
        {
            return Ok();
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
            if (await this._connectRepository.SaveAll())
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }
    }
}