// ITokenStorage.cs
namespace OnlineSinav.MVC.Services
{
    public interface ITokenStorage
    {
        void StoreToken(string token);
        string? GetToken();
        void RemoveToken();
    }
}