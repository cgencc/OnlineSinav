using System.ComponentModel.DataAnnotations;

namespace OnlineSinav.API.DTOs
{
    public class UserRegisterDto
    {
        [Required] public string FullName { get; set; } = string.Empty;
        [Required] public string UserName { get; set; } = string.Empty;
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [Required, MinLength(6)] public string Password { get; set; } = string.Empty;
        public string? StudentNumber { get; set; }
    }

    public class UserLoginDto
    {
        [Required] public string UserName { get; set; } = string.Empty;
        [Required] public string Password { get; set; } = string.Empty;
    }

    public class ResultDto
    {
        public bool Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
    }
}