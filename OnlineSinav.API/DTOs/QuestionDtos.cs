using System.ComponentModel.DataAnnotations;

namespace OnlineSinav.API.DTOs
{
    public class QuestionAddDto
    {
        [Required] public int ExamId { get; set; }
        [Required] public string QuestionText { get; set; } = string.Empty;
        public int Points { get; set; } = 10;
    }

    public class OptionAddDto
    {
        [Required] public int ExamQuestionId { get; set; }
        [Required] public string OptionText { get; set; } = string.Empty;
        [Required] public bool IsCorrect { get; set; } 
    }
    public class QuestionUpdateDto
    {
        public int Id { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public int Points { get; set; }
    }
}