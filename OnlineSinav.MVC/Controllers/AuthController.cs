using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineSinav.MVC.Models.DTOs;
using OnlineSinav.MVC.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace OnlineSinav.MVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ITokenStorage _tokenStorage;

        public AuthController(ApiService apiService, ITokenStorage tokenStorage)
        {
            _apiService = apiService;
            _tokenStorage = tokenStorage;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(UserLoginDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _apiService.PostAsync<ResultDto>("Auth/Login", model);

            System.Diagnostics.Debug.WriteLine("Result: " + (result != null ? result.Status.ToString() : "null"));

            if (result != null && result.Status)
            {
                var token = result.Data?.ToString();
                if (!string.IsNullOrEmpty(token))
                {

                    _tokenStorage.StoreToken(token);

                   
                    await SignInWithToken(token);

                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError("", result?.Message ?? "Giriş başarısız.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(UserRegisterDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _apiService.PostAsync<ResultDto>("Auth/Register", model);

            if (result != null && result.Status)
            {
                TempData["SuccessMessage"] = "Kayıt başarılı! Giriş yapabilirsiniz.";
                return RedirectToAction("Login");
            }

            ModelState.AddModelError("", result?.Message ?? "Kayıt başarısız.");
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            _tokenStorage.RemoveToken();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        private async Task SignInWithToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var claims = jwtToken.Claims.ToList();

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }


    }
}