namespace OnlineSinav.API.Models
{
    public class ExamResult : AppBaseEntity
    {
        public int Score { get; set; } 

        public int ExamId { get; set; }
        public Exam? Exam { get; set; }

        public string AppUserId { get; set; } = string.Empty;
        public AppUser? AppUser { get; set; }
    }
}