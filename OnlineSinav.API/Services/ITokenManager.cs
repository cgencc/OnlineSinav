using OnlineSinav.API.Models;

namespace OnlineSinav.API.Services
{
    public interface ITokenManager
    {
        string GenerateToken(AppUser user, IList<string> roles);
    }
}