using Microsoft.AspNetCore.Identity;

namespace OnlineSinav.API.Models
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? StudentNumber { get; set; }


        public ICollection<ExamResult>? ExamResults { get; set; }
    }
}