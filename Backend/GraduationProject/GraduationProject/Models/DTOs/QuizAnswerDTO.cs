namespace GraduationProject.Models.DTOs
{
    public class AnswerDTO 
    {
        public int Id { get; set; }
        public string AnswerText { get; set; } = null!;
        public int Position { get; set; }
        public bool IsCorrect { get; set; }

    }
}
