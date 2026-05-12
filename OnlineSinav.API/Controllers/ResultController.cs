using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSinav.API.Data;
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
        private readonly IGenericRepository<StudentAnswer> _answerRepo;

        public ResultController(
            IGenericRepository<ExamResult> resultRepo,
            IGenericRepository<Exam> examRepo,
            IGenericRepository<StudentAnswer> answerRepo)
        {
            _resultRepo = resultRepo;
            _examRepo = examRepo;
            _answerRepo = answerRepo;
        }

        [Authorize(Roles = "Student")]
        [HttpPost("Submit")]
        public async Task<IActionResult> SubmitExam(ExamSubmitDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var alreadyTaken = await _resultRepo.AsQueryable()
                .AnyAsync(r => r.ExamId == model.ExamId && r.AppUserId == userId);

            if (alreadyTaken)
                return BadRequest(new ResultDto { Status = false, Message = "Bu sinava daha once katildiniz!" });

            var exam = await _examRepo.AsQueryable()
                .Include(e => e.Questions!).ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(e => e.Id == model.ExamId);

            if (exam == null || !exam.IsActive)
                return NotFound(new ResultDto { Status = false, Message = "Sinav bulunamadi veya pasif." });

            var now = DateTime.Now;
            if (exam.StartDate.HasValue && exam.StartDate > now)
                return BadRequest(new ResultDto { Status = false, Message = "Sinav henuz baslamadi." });
            if (exam.EndDate.HasValue && exam.EndDate < now)
                return BadRequest(new ResultDto { Status = false, Message = "Sinav suresi doldu." });

            int totalScore = 0;
            var answerEntities = new List<StudentAnswer>();

            foreach (var studentAnswer in model.Answers)
            {
                var question = exam.Questions!.FirstOrDefault(q => q.Id == studentAnswer.QuestionId);
                if (question == null) continue;

                var selectedOption = question.Options!.FirstOrDefault(o => o.Id == studentAnswer.SelectedOptionId);
                bool isCorrect = selectedOption != null && selectedOption.IsCorrect;

                if (isCorrect)
                    totalScore += question.Points;

                answerEntities.Add(new StudentAnswer
                {
                    QuestionId = studentAnswer.QuestionId,
                    SelectedOptionId = studentAnswer.SelectedOptionId,
                    IsCorrect = isCorrect,
                    CreatedDate = DateTime.Now,
                    IsActive = true
                });
            }

            var examResult = new ExamResult
            {
                ExamId = exam.Id,
                AppUserId = userId!,
                Score = totalScore,
                CreatedDate = DateTime.Now,
                IsActive = true,
                Answers = answerEntities
            };

            await _resultRepo.AddAsync(examResult);
            await _resultRepo.SaveAsync();

            return Ok(new ResultDto
            {
                Status = true,
                Message = "Sinavi tamamladiniz! Puaniniz: " + totalScore,
                Data = new { score = totalScore, resultId = examResult.Id }
            });
        }

        // Ogrenci: kendi tum sinav gecmisi
        [Authorize(Roles = "Student")]
        [HttpGet("MyResults")]
        public async Task<IActionResult> GetMyResults()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var results = await _resultRepo.AsQueryable()
                .Include(r => r.Exam)
                .Where(r => r.AppUserId == userId)
                .OrderByDescending(r => r.CreatedDate)
                .Select(r => new
                {
                    resultId = r.Id,
                    examTitle = r.Exam!.Title,
                    score = r.Score,
                    date = r.CreatedDate
                }).ToListAsync();

            return Ok(new ResultDto { Status = true, Data = results });
        }

        // Ogrenci: tek sinav detayi - hangi soruda ne sectigini goster
        [Authorize(Roles = "Student")]
        [HttpGet("Detail/{resultId}")]
        public async Task<IActionResult> GetResultDetail(int resultId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _resultRepo.AsQueryable()
                .Include(r => r.Exam)
                .Include(r => r.Answers!)
                    .ThenInclude(a => a.Question)
                        .ThenInclude(q => q!.Options)
                .FirstOrDefaultAsync(r => r.Id == resultId && r.AppUserId == userId);

            if (result == null)
                return NotFound(new ResultDto { Status = false, Message = "Sonuc bulunamadi." });

            var detail = new
            {
                examTitle = result.Exam!.Title,
                score = result.Score,
                date = result.CreatedDate,
                answers = result.Answers!.Select(a => new
                {
                    questionText = a.Question!.QuestionText,
                    points = a.Question.Points,
                    isCorrect = a.IsCorrect,
                    selectedOptionText = a.SelectedOption != null ? a.SelectedOption.OptionText : "",
                    correctOptionText = a.Question.Options!
                                            .Where(o => o.IsCorrect)
                                            .Select(o => o.OptionText)
                                            .FirstOrDefault() ?? ""
                })
            };

            return Ok(new ResultDto { Status = true, Data = detail });
        }

        // Ogretmen: bir sinavin tum ogrenci sonuclari (liste)
        [Authorize(Roles = "Teacher")]
        [HttpGet("Exam/{examId}")]
        public async Task<IActionResult> GetResultsByExam(int examId)
        {
            var results = await _resultRepo.AsQueryable()
                .Include(r => r.AppUser)
                .Where(r => r.ExamId == examId)
                .OrderByDescending(r => r.CreatedDate)
                .Select(r => new
                {
                    resultId = r.Id,
                    studentName = r.AppUser!.FullName,
                    studentNumber = r.AppUser.StudentNumber,
                    score = r.Score,
                    submitDate = r.CreatedDate
                }).ToListAsync();

            return Ok(new ResultDto { Status = true, Data = results });
        }

        // Ogretmen: bir ogrencinin o sinava verdigi cevaplarin detayi
        [Authorize(Roles = "Teacher")]
        [HttpGet("ExamDetail/{resultId}")]
        public async Task<IActionResult> GetStudentResultDetail(int resultId)
        {
            var result = await _resultRepo.AsQueryable()
                .Include(r => r.AppUser)
                .Include(r => r.Exam)
                .Include(r => r.Answers!)
                    .ThenInclude(a => a.Question)
                        .ThenInclude(q => q!.Options)
                .Include(r => r.Answers!)
                    .ThenInclude(a => a.SelectedOption)
                .FirstOrDefaultAsync(r => r.Id == resultId);

            if (result == null)
                return NotFound(new ResultDto { Status = false, Message = "Sonuc bulunamadi." });

            var detail = new
            {
                studentName = result.AppUser!.FullName,
                studentNumber = result.AppUser.StudentNumber,
                examTitle = result.Exam!.Title,
                score = result.Score,
                submitDate = result.CreatedDate,
                answers = result.Answers!.Select(a => new
                {
                    questionText = a.Question!.QuestionText,
                    points = a.Question.Points,
                    isCorrect = a.IsCorrect,
                    selectedOptionText = a.SelectedOption != null ? a.SelectedOption.OptionText : "-",
                    correctOptionText = a.Question.Options!
                                            .Where(o => o.IsCorrect)
                                            .Select(o => o.OptionText)
                                            .FirstOrDefault() ?? ""
                })
            };

            return Ok(new ResultDto { Status = true, Data = detail });
        }
    }
}