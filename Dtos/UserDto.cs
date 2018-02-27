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
        [StringLength(20, MinimumLength = 8, ErrorMessage = "password should be minimum of 8 characters")]
        public string Password { get; set; }
    }
}