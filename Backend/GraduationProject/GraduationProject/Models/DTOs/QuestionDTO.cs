namespace GraduationProject.Models.DTOs
{
    public class QuestionDTO 
    {
        public int Id { get; set; }
        public string QuestionText { get; set; } = null!;
        public int Position { get; set; }
        public IEnumerable<AnswerDTO> Answers { get; set; } = null!;

    }
}
