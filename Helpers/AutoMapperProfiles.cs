using System.Linq;
using AutoMapper;
using connect_api.Dtos;
using ConnectApi.Dtos;
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

            //fetch phoyo
            CreateMap<PhotoCreationDto, Photo>();
            CreateMap<Photo, PhotoFetchDto>();

            //registration dto
            CreateMap<UserDto, User>();
        }
    }
}