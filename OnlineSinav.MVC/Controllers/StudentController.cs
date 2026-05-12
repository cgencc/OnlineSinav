using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OnlineSinav.MVC.Models.DTOs;
using OnlineSinav.MVC.Services;

namespace OnlineSinav.MVC.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly ApiService _api;
        public StudentController(ApiService api) => _api = api;

        public async Task<IActionResult> Exams()
        {
            var exams = await _api.GetAsync<List<ExamListDto>>("Exam");
            return View(exams ?? new List<ExamListDto>());
        }

        public async Task<IActionResult> TakeExam(int id)
        {
            var exam = await _api.GetAsync<ExamDetailDto>($"Exam/{id}");
            if (exam == null) return NotFound();
            return View(exam);
        }

        public async Task<IActionResult> MyResults()
        {
            var result = await _api.GetAsync<ResultDto>("Result/MyResults");
            List<MyResultDto> items = new();
            if (result != null && result.Status && result.Data != null)
            {
                var json = JsonConvert.SerializeObject(result.Data);
                items = JsonConvert.DeserializeObject<List<MyResultDto>>(json) ?? new();
            }
            return View(items);
        }


        [HttpGet]
        public async Task<IActionResult> GetResultDetail(int resultId)
        {
            var raw = await _api.GetRawAsync($"Result/Detail/{resultId}");
            return Content(raw, "application/json");
        }

        [HttpPost]
        public async Task<IActionResult> SubmitExam([FromBody] ExamSubmitDto model)
        {
            var result = await _api.PostAsync<ResultDto>("Result/Submit", model);
            return Json(result);
        }
    }
}   