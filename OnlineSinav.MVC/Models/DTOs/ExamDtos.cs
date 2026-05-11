namespace OnlineSinav.MVC.Models.DTOs
{
    public class ExamListDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int DurationInMinutes { get; set; }
        public DateTime CreatedDate { get; set; }
        public int QuestionCount { get; set; }
    }

    public class ExamDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int DurationInMinutes { get; set; }
        public List<QuestionDto> Questions { get; set; } = new();
    }

    public class QuestionDto
    {
        public int Id { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public int Points { get; set; }
        public List<OptionDto> Options { get; set; } = new();
    }

    public class OptionDto
    {
        public int Id { get; set; }
        public string OptionText { get; set; } = string.Empty;
    }

    // Cevap gönderme için (API ile aynı yapı)
    public class ExamSubmitDto
    {
        public int ExamId { get; set; }
        public List<StudentAnswerDto> Answers { get; set; } = new();
    }

    public class StudentAnswerDto
    {
        public int QuestionId { get; set; }
        public int SelectedOptionId { get; set; }
    }
    public class ExamCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DurationInMinutes { get; set; }
    }

    public class ExamUpdateDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DurationInMinutes { get; set; }
        public bool IsActive { get; set; }
    }

    public class QuestionAddDto
    {
        public int ExamId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public int Points { get; set; } = 10;
    }

    public class OptionAddDto
    {
        public int ExamQuestionId { get; set; }
        public string OptionText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }

    public class QuestionUpdateDto
    {
        public int Id { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public int Points { get; set; }
    }

    public class ExamResultItemDto
    {
        public string StudentName { get; set; } = string.Empty;
        public string StudentNumber { get; set; } = string.Empty;
        public int Score { get; set; }
        public DateTime SubmitDate { get; set; }
    }
}