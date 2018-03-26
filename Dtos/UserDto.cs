using System;
using System.ComponentModel.DataAnnotations;

namespace ConnectApi.Dtos
{
    public class UserDto
    {
        [Required]
        public string Firstname { get; set; }
        [Required]
        public string Lastname { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Gender { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; }
        [Required]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "password should be minimum of 8 characters")]
        public string Password { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastActive { get; set; }

        public UserDto()
        {
            Created = DateTime.Now;
            LastActive = DateTime.Now;
        }
    }
}