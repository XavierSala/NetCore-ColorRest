using System;
using System.ComponentModel.DataAnnotations;

namespace colorsRest.Controllers
{
    public class TokenResult
    {
        public string Token { get; set; }
    }

    public class Error
    {
        public string Message { get; set; }
    }

    public class LoginDto
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

    }

    public class RegisterDto
    {
        [Required]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "PASSWORD_MIN_LENGTH", MinimumLength = 6)]
        public string Password { get; set; }
    }

}
