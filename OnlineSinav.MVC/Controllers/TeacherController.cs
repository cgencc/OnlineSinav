using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineSinav.MVC.Models.DTOs;
using OnlineSinav.MVC.Services;

namespace OnlineSinav.MVC.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherController : Controller
    {
        private readonly ApiService _api;
        public TeacherController(ApiService api) => _api = api;

        public IActionResult Index() => View();
        public IActionResult Create() => View();
        public IActionResult Edit(int id) => View("Create", id);
        public IActionResult Questions(int examId) => View(model: examId);
        public IActionResult Results(int examId) => View(model: examId);
        public IActionResult UserManagement() => View();

        [HttpGet]
        public async Task<IActionResult> GetExams()
            => Json(await _api.GetAsync<List<ExamListDto>>("Exam?includeInactive=true"));

        [HttpGet]
        public async Task<IActionResult> GetExam(int id)
            => Json(await _api.GetAsync<ExamDetailDto>($"Exam/{id}"));

        [HttpPost]
        public async Task<IActionResult> CreateExam([FromBody] ExamCreateDto model)
            => Json(await _api.PostAsync<ResultDto>("Exam", model));

        [HttpPut]
        public async Task<IActionResult> UpdateExam([FromBody] ExamUpdateDto model)
            => Json(await _api.PutAsync<ResultDto>("Exam", model));

        [HttpDelete]
        public async Task<IActionResult> DeleteExam(int id)
            => Json(await _api.DeleteAsync<ResultDto>($"Exam/{id}"));

        [HttpPost]
        public async Task<IActionResult> AddQuestion([FromBody] QuestionAddDto model)
            => Json(await _api.PostAsync<ResultDto>("Exam/AddQuestion", model));

        [HttpPost]
        public async Task<IActionResult> AddOption([FromBody] OptionAddDto model)
            => Json(await _api.PostAsync<ResultDto>("Exam/AddOption", model));

        [HttpGet]
        public async Task<IActionResult> GetExamResults(int examId)
        {
            var raw = await _api.GetRawAsync($"Result/Exam/{examId}");
            return Content(raw, "application/json");
        }

        [HttpGet]
        public async Task<IActionResult> GetStudentResultDetail(int resultId)
        {
            var raw = await _api.GetRawAsync($"Result/ExamDetail/{resultId}");
            return Content(raw, "application/json");
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var raw = await _api.GetRawAsync("Auth/GetUsers");
            return Content(raw, "application/json");
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteQuestion(int id)
            => Json(await _api.DeleteAsync<ResultDto>($"Exam/DeleteQuestion/{id}"));

        [HttpPut]
        public async Task<IActionResult> UpdateQuestion([FromBody] QuestionUpdateDto model)
            => Json(await _api.PutAsync<ResultDto>("Exam/UpdateQuestion", model));

        [HttpPost]
        public async Task<IActionResult> MakeTeacher([FromBody] UserRoleDto model)
            => Json(await _api.PostAsync<ResultDto>($"Auth/MakeTeacher?userName={model.UserName}", null));

        [HttpPost]
        public async Task<IActionResult> RevokeTeacher([FromBody] UserRoleDto model)
            => Json(await _api.PostAsync<ResultDto>($"Auth/RevokeTeacher?userName={model.UserName}", null));
    }
}