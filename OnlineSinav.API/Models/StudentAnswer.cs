namespace OnlineSinav.API.Models
{
    // Stores each answer a student gave during an exam attempt.
    // Linked to ExamResult so we can show the full breakdown later.
    public class StudentAnswer : AppBaseEntity
    {
        public int ExamResultId { get; set; }
        public ExamResult? ExamResult { get; set; }

        public int QuestionId { get; set; }
        public ExamQuestion? Question { get; set; }

        public int SelectedOptionId { get; set; }
        public QuestionOption? SelectedOption { get; set; }

        public bool IsCorrect { get; set; }
    }
}