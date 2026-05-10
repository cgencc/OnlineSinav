namespace OnlineSinav.API.Models
{
    public class QuestionOption : AppBaseEntity
    {
        public string OptionText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; } = false; 

        public int ExamQuestionId { get; set; }
        public ExamQuestion? ExamQuestion { get; set; }
    }
}