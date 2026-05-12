namespace OnlineSinav.API.Models
{
    public class Exam : AppBaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DurationInMinutes { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string AppUserId { get; set; } = string.Empty;
        public AppUser? AppUser { get; set; }

        public ICollection<ExamQuestion>? Questions { get; set; }
    }
}