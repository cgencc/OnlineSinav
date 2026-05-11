// CookieTokenStorage.cs
using Microsoft.AspNetCore.Http;
using System;
using OnlineSinav.MVC.Services;

namespace OnlineSinav.MVC.Services
{
    public class CookieTokenStorage : ITokenStorage
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string CookieName = "AuthToken";

        public CookieTokenStorage(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void StoreToken(string token)
        {
            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,                     // ← HTTP için false
                SameSite = SameSiteMode.Lax,        // ← Strict yerine Lax
                Expires = DateTime.Now.AddHours(3)
            };
            _httpContextAccessor.HttpContext?.Response.Cookies.Append(CookieName, token, options);
        }

        public string? GetToken()
        {
            return _httpContextAccessor.HttpContext?.Request.Cookies[CookieName];
        }

        public void RemoveToken()
        {
            _httpContextAccessor.HttpContext?.Response.Cookies.Delete(CookieName);
        }
    }
}