using System.ComponentModel.DataAnnotations;

namespace UserService.DTOs
{
    public class RegisterUserDto
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string FullName { get; set; }
    }
}
