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
    public class ExamController : ControllerBase
    {
        private readonly IGenericRepository<Exam> _examRepo;

        public ExamController(IGenericRepository<Exam> examRepo)
        {
            _examRepo = examRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(string? search = null)
        {
            var query = _examRepo.AsQueryable().Where(e => e.IsActive);
            if (!string.IsNullOrEmpty(search))
                query = query.Where(e => e.Title.ToLower().Contains(search.ToLower()));

            var exams = await query.Select(e => new ExamListDto
            {
                Id = e.Id,
                Title = e.Title,
                DurationInMinutes = e.DurationInMinutes,
                CreatedDate = e.CreatedDate,
                QuestionCount = e.Questions!.Count
            }).ToListAsync();

            return Ok(exams);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var exam = await _examRepo.AsQueryable()
                .Include(e => e.Questions!).ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(e => e.Id == id && e.IsActive);

            if (exam == null)
                return NotFound(new ResultDto { Status = false, Message = "Sınav bulunamadı." });

            var safeExamData = new
            {
                id = exam.Id,
                title = exam.Title,
                description = exam.Description,
                durationInMinutes = exam.DurationInMinutes,
                questions = exam.Questions!.Select(q => new
                {
                    id = q.Id,
                    questionText = q.QuestionText,
                    points = q.Points,
                    options = q.Options!.Select(o => new { id = o.Id, optionText = o.OptionText })
                })
            };

            return Ok(safeExamData);
        }

        [Authorize(Roles = "Teacher")]
        [HttpPost]
        public async Task<IActionResult> Create(ExamCreateDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var exam = new Exam
            {
                Title = model.Title,
                Description = model.Description,
                DurationInMinutes = model.DurationInMinutes,
                AppUserId = userId!,
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            await _examRepo.AddAsync(exam);
            await _examRepo.SaveAsync();
            return Ok(new ResultDto { Status = true, Message = "Sınav oluşturuldu.", Data = exam.Id });
        }

        [Authorize(Roles = "Teacher")]
        [HttpPost("AddQuestion")]
        public async Task<IActionResult> AddQuestion(QuestionAddDto model, [FromServices] IGenericRepository<ExamQuestion> questionRepo)
        {
            // Aynı sınavdaki aktif soruların toplam puanını hesapla
            var currentTotal = await questionRepo.AsQueryable()
                .Where(q => q.ExamId == model.ExamId && q.IsActive)
                .SumAsync(q => q.Points);

            if (currentTotal + model.Points > 100)
                return BadRequest(new ResultDto { Status = false, Message = $"Sınav toplam puanı 100'ü aşıyor! Kalan puan: {100 - currentTotal}" });

            var question = new ExamQuestion
            {
                ExamId = model.ExamId,
                QuestionText = model.QuestionText,
                Points = model.Points,
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            await questionRepo.AddAsync(question);
            await questionRepo.SaveAsync();
            return Ok(new ResultDto { Status = true, Message = "Soru eklendi.", Data = question.Id });
        }

        [Authorize(Roles = "Teacher")]
        [HttpPost("AddOption")]
        public async Task<IActionResult> AddOption(OptionAddDto model,
            [FromServices] IGenericRepository<QuestionOption> optionRepo)
        {
            var option = new QuestionOption
            {
                ExamQuestionId = model.ExamQuestionId,
                OptionText = model.OptionText,
                IsCorrect = model.IsCorrect,
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            await optionRepo.AddAsync(option);
            await optionRepo.SaveAsync();
            return Ok(new ResultDto { Status = true, Message = "Şık eklendi." });
        }

        [Authorize(Roles = "Teacher")]
        [HttpPut]
        public async Task<IActionResult> Update(ExamUpdateDto model)
        {
            var exam = await _examRepo.GetByIdAsync(model.Id);
            if (exam == null)
                return NotFound(new ResultDto { Status = false, Message = "Güncellenecek sınav bulunamadı." });

            exam.Title = model.Title;
            exam.Description = model.Description;
            exam.DurationInMinutes = model.DurationInMinutes;
            exam.IsActive = model.IsActive;

            _examRepo.Update(exam);
            await _examRepo.SaveAsync();
            return Ok(new ResultDto { Status = true, Message = "Sınav başarıyla güncellendi." });
        }

        [Authorize(Roles = "Teacher")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var exam = await _examRepo.GetByIdAsync(id);
            if (exam == null)
                return NotFound(new ResultDto { Status = false, Message = "Silinecek sınav bulunamadı." });

            exam.IsActive = false;
            _examRepo.Update(exam);
            await _examRepo.SaveAsync();
            return Ok(new ResultDto { Status = true, Message = "Sınav pasif hale getirildi." });
        }
        [Authorize(Roles = "Teacher")]
        [HttpDelete("DeleteQuestion/{id}")]
        public async Task<IActionResult> DeleteQuestion(int id, [FromServices] IGenericRepository<ExamQuestion> questionRepo)
        {
            var question = await questionRepo.GetByIdAsync(id);
            if (question == null)
                return NotFound(new ResultDto { Status = false, Message = "Soru bulunamadı." });

            question.IsActive = false;
            questionRepo.Update(question);
            await questionRepo.SaveAsync();

            return Ok(new ResultDto { Status = true, Message = "Soru silindi." });
        }   
        [Authorize(Roles = "Teacher")]
        [HttpPut("UpdateQuestion")]
        public async Task<IActionResult> UpdateQuestion(QuestionUpdateDto model, [FromServices] IGenericRepository<ExamQuestion> questionRepo)
        {
            var question = await questionRepo.GetByIdAsync(model.Id);
            if (question == null)
                return NotFound(new ResultDto { Status = false, Message = "Soru bulunamadı." });

            // Aynı sınavdaki diğer soruların toplam puanını hesapla (güncellenen hariç)
            var otherQuestionsTotal = await questionRepo.AsQueryable()
                .Where(q => q.ExamId == question.ExamId && q.Id != model.Id && q.IsActive)
                .SumAsync(q => q.Points);

            if (otherQuestionsTotal + model.Points > 100)
                return BadRequest(new ResultDto { Status = false, Message = $"Sınav toplam puanı 100'ü aşıyor! Kalan puan: {100 - otherQuestionsTotal}" });

            question.QuestionText = model.QuestionText;
            question.Points = model.Points;
            questionRepo.Update(question);
            await questionRepo.SaveAsync();

            return Ok(new ResultDto { Status = true, Message = "Soru güncellendi." });
        }

    }
}