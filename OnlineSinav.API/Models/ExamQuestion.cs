namespace OnlineSinav.API.Models
{
    public class ExamQuestion : AppBaseEntity
    {
        public string QuestionText { get; set; } = string.Empty;
        public int Points { get; set; } = 10;


        public int ExamId { get; set; }
        public Exam? Exam { get; set; }


        public ICollection<QuestionOption>? Options { get; set; }
    }
}