using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineSinav.MVC.Services;
using OnlineSinav.MVC.Models.DTOs;

namespace OnlineSinav.MVC.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherController : Controller
    {
        private readonly ApiService _api;

        public TeacherController(ApiService api)
        {
            _api = api;
        }

        // --- Sayfalar ---
        public IActionResult Index() => View();
        public IActionResult Create() => View();
        public IActionResult Edit(int id) => View("Create", id);
        public IActionResult Questions(int examId) => View(model: examId);
        public IActionResult Results(int examId) => View(model: examId);
        public IActionResult UserManagement() => View();   // yeni

        // --- Sınav listesini JSON döndür (AJAX) ---
        [HttpGet]
        public async Task<IActionResult> GetExams()
        {
            var exams = await _api.GetAsync<List<ExamListDto>>("Exam");
            return Json(exams);
        }

        // --- Tek sınav detayı (düzenleme için) ---
        [HttpGet]
        public async Task<IActionResult> GetExam(int id)
        {
            var exam = await _api.GetAsync<ExamDetailDto>($"Exam/{id}");
            return Json(exam);
        }

        // --- Sınav oluştur ---
        [HttpPost]
        public async Task<IActionResult> CreateExam([FromBody] ExamCreateDto model)
        {
            var result = await _api.PostAsync<ResultDto>("Exam", model);
            return Json(result);
        }

        // --- Sınav güncelle ---
        [HttpPut]
        public async Task<IActionResult> UpdateExam([FromBody] ExamUpdateDto model)
        {
            var result = await _api.PutAsync<ResultDto>($"Exam/{model.Id}", model);
            return Json(result);
        }

        // --- Sınav sil (pasif yap) ---
        [HttpDelete]
        public async Task<IActionResult> DeleteExam(int id)
        {
            var result = await _api.DeleteAsync<ResultDto>($"Exam/{id}");
            return Json(result);
        }

        // --- Soru ekle ---
        [HttpPost]
        public async Task<IActionResult> AddQuestion([FromBody] QuestionAddDto model)
        {
            var result = await _api.PostAsync<ResultDto>("Exam/AddQuestion", model);
            return Json(result);
        }

        // --- Şık ekle ---
        [HttpPost]
        public async Task<IActionResult> AddOption([FromBody] OptionAddDto model)
        {
            var result = await _api.PostAsync<ResultDto>("Exam/AddOption", model);
            return Json(result);
        }

        // --- Sınav sonuçlarını getir ---
        [HttpGet]
        public async Task<IActionResult> GetExamResults(int examId)
        {
            var result = await _api.GetAsync<ResultDto>($"Result/Exam/{examId}");
            return Json(result);
        }

        // ============= KULLANICI YÖNETİMİ =============
        // Kullanıcı listesini getir (JSON)
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var result = await _api.GetAsync<ResultDto>("Auth/GetUsers");
            return Json(result);
        }

        // Kullanıcıya öğretmen yetkisi ver
        [HttpPost]
        public async Task<IActionResult> MakeTeacher([FromBody] UserRoleDto model)
        {
            var result = await _api.PostAsync<ResultDto>($"Auth/MakeTeacher?userName={model.UserName}", null);
            return Json(result);
        }

        // Kullanıcının öğretmen yetkisini al
        [HttpPost]
        public async Task<IActionResult> RevokeTeacher([FromBody] UserRoleDto model)
        {
            var result = await _api.PostAsync<ResultDto>($"Auth/RevokeTeacher?userName={model.UserName}", null);
            return Json(result);
        }
    }
}