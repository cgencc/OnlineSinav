using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSinav.API.DTOs;
using OnlineSinav.API.Models;
using OnlineSinav.API.Repositories;
using System.Security.Claims;

namespace OnlineSinav.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class ResultController : ControllerBase
    {
        private readonly IGenericRepository<ExamResult> _resultRepo;
        private readonly IGenericRepository<Exam> _examRepo;

        public ResultController(IGenericRepository<ExamResult> resultRepo, IGenericRepository<Exam> examRepo)
        {
            _resultRepo = resultRepo;
            _examRepo = examRepo;
        }


        [Authorize(Roles = "Student")]
        [HttpPost("Submit")]
        public async Task<IActionResult> SubmitExam(ExamSubmitDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);


            var alreadyTaken = await _resultRepo.AsQueryable()
                .AnyAsync(r => r.ExamId == model.ExamId && r.AppUserId == userId);

            if (alreadyTaken)
                return BadRequest(new ResultDto { Status = false, Message = "Bu sınava daha önce katıldınız! İkinci kez girilemez." });


            var exam = await _examRepo.AsQueryable()
                .Include(e => e.Questions!)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(e => e.Id == model.ExamId);

            if (exam == null || !exam.IsActive)
                return NotFound(new ResultDto { Status = false, Message = "Sınav bulunamadı veya pasif." });


            int totalScore = 0;

            foreach (var studentAnswer in model.Answers)
            {

                var question = exam.Questions!.FirstOrDefault(q => q.Id == studentAnswer.QuestionId);
                if (question != null)
                {

                    var selectedOption = question.Options!.FirstOrDefault(o => o.Id == studentAnswer.SelectedOptionId);

                    if (selectedOption != null && selectedOption.IsCorrect)
                    {
                        totalScore += question.Points;
                    }
                }
            }

            var examResult = new ExamResult
            {
                ExamId = exam.Id,
                AppUserId = userId!,
                Score = totalScore,
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            await _resultRepo.AddAsync(examResult);
            await _resultRepo.SaveAsync();

            return Ok(new ResultDto
            {
                Status = true,
                Message = $"Sınavı tamamladınız! Toplam Puanınız: {totalScore}",
                Data = totalScore
            });
        }

        [Authorize(Roles = "Teacher")]
        [HttpGet("Exam/{examId}")]
        public async Task<IActionResult> GetResultsByExam(int examId)
        {
            var results = await _resultRepo.AsQueryable()
                .Include(r => r.AppUser) 
                .Where(r => r.ExamId == examId)
                .Select(r => new
                {
                    StudentName = r.AppUser!.FullName,
                    StudentNumber = r.AppUser.StudentNumber,
                    Score = r.Score,
                    SubmitDate = r.CreatedDate
                }).ToListAsync();

            return Ok(new ResultDto { Status = true, Message = "Sınav sonuçları listelendi.", Data = results });
        }
        [Authorize(Roles = "Student")]
        [HttpGet("MyResults")]
        public async Task<IActionResult> GetMyResults()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var results = await _resultRepo.AsQueryable()
                .Include(r => r.Exam)
                .Where(r => r.AppUserId == userId)
                .Select(r => new
                {
                    ExamTitle = r.Exam!.Title,
                    Score = r.Score,
                    Date = r.CreatedDate
                }).ToListAsync();

            return Ok(new ResultDto { Status = true, Data = results });
        }
    }
}