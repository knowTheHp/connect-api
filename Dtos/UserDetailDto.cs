using System;
using System.Collections.Generic;

namespace connect_api.Dtos
{
    public class UserDetailDto
    {
        public int Id { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public int Age { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastActive { get; set; }
        public string Introduction { get; set; }
        public string Interests { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PhotoUrl { get; set; }
        public ICollection<EducationDetailDto> Education { get; set; }
        public ICollection<WorkExperienceDetailDto> WorkExperiences { get; set; }
        public ICollection<SkillDetailDto> Skills { get; set; }
        public ICollection<ProjectDetailDto> Projects { get; set; }
        public ICollection<PhotoDetailDto> Photos { get; set; }
    }
}