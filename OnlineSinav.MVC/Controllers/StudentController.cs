using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineSinav.MVC.Models.DTOs;
using OnlineSinav.MVC.Services;

namespace OnlineSinav.MVC.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly ApiService _api;

        public StudentController(ApiService api)
        {
            _api = api;
        }

        // Sınav listesi
        public async Task<IActionResult> Exams()
        {
            var exams = await _api.GetAsync<List<ExamListDto>>("Exam");
            return View(exams);
        }

        // Sınava girme sayfası (sınav ID ile)
        public async Task<IActionResult> TakeExam(int id)
        {
            var exam = await _api.GetAsync<ExamDetailDto>($"Exam/{id}");
            if (exam == null)
                return NotFound();

            // Daha önce girilmiş mi kontrolü: API'den alabiliriz ama şimdilik view tarafında hata gösteririz.
            return View(exam);
        }
    }
}