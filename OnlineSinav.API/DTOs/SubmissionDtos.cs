namespace OnlineSinav.API.DTOs
{
    public class StudentAnswerDto
    {
        public int QuestionId { get; set; }
        public int SelectedOptionId { get; set; }
    }

    public class ExamSubmitDto
    {
        public int ExamId { get; set; }
        public List<StudentAnswerDto> Answers { get; set; } = new List<StudentAnswerDto>();
    }
}