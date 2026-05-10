using System.ComponentModel.DataAnnotations;

namespace OnlineSinav.API.DTOs
{
    public class ExamCreateDto
    {
        [Required(ErrorMessage = "Sınav başlığı zorunludur.")]
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        [Required]
        public int DurationInMinutes { get; set; } 
    }

    public class ExamListDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int DurationInMinutes { get; set; }
        public DateTime CreatedDate { get; set; }
        public int QuestionCount { get; set; }
    }

    public class ExamUpdateDto
    {
        public int Id { get; set; }
        [Required] public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required] public int DurationInMinutes { get; set; }
        public bool IsActive { get; set; }
    }
}