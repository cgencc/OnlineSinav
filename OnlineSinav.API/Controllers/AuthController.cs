using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSinav.API.DTOs;
using OnlineSinav.API.Models;
using OnlineSinav.API.Services;

namespace OnlineSinav.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenManager _tokenManager;

        public AuthController(UserManager<AppUser> userManager, ITokenManager tokenManager)
        {
            _userManager = userManager;
            _tokenManager = tokenManager;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserRegisterDto model)
        {
            var user = new AppUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FullName = model.FullName,
                StudentNumber = model.StudentNumber
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Student");
                return Ok(new ResultDto { Status = true, Message = "Kayıt başarılı. Giriş yapabilirsiniz." });
            }
            return BadRequest(new ResultDto { Status = false, Message = "Kayıt başarısız.", Data = result.Errors });
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserLoginDto model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var roles = await _userManager.GetRolesAsync(user);
                var token = _tokenManager.GenerateToken(user, roles);
                return Ok(new ResultDto { Status = true, Message = "Giriş başarılı.", Data = token });
            }
            return Unauthorized(new ResultDto { Status = false, Message = "Kullanıcı adı veya şifre hatalı." });
        }

        [Authorize(Roles = "Teacher")]
        [HttpPost("MakeTeacher")]
        public async Task<IActionResult> MakeTeacher(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
                return NotFound(new ResultDto { Status = false, Message = "Kullanıcı bulunamadı." });

            await _userManager.AddToRoleAsync(user, "Teacher");

            return Ok(new ResultDto { Status = true, Message = $"{userName} artık bir öğretmen/yönetici!" });
        }

        [Authorize(Roles = "Teacher")]
        [HttpPost("RevokeTeacher")]
        public async Task<IActionResult> RevokeTeacher(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
                return NotFound(new ResultDto { Status = false, Message = "Kullanıcı bulunamadı." });

            var isTeacher = await _userManager.IsInRoleAsync(user, "Teacher");
            if (!isTeacher)
                return BadRequest(new ResultDto { Status = false, Message = "Bu kullanıcı zaten öğretmen değil." });

            var result = await _userManager.RemoveFromRoleAsync(user, "Teacher");

            if (result.Succeeded)
            {
                return Ok(new ResultDto { Status = true, Message = $"{userName} adlı kullanıcının öğretmen yetkisi başarıyla alındı!" });
            }

            return BadRequest(new ResultDto { Status = false, Message = "Yetki alınırken bir sorun oluştu." });
        }
        [Authorize(Roles = "Teacher")]
        [HttpGet("GetUsers")]
        public async Task<IActionResult> GetUsers()
        {
            // Tüm kullanıcıları al
            var allUsers = await _userManager.Users.ToListAsync();

            var userList = new List<object>();
            foreach (var u in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(u);
                var mainRole = roles.FirstOrDefault() ?? "Student";

                userList.Add(new
                {
                    id = u.Id,
                    userName = u.UserName,
                    fullName = u.FullName,
                    email = u.Email,
                    studentNumber = u.StudentNumber ?? "",
                    rol = mainRole
                });
            }

            return Ok(new ResultDto { Status = true, Data = userList });
        }
    }
}