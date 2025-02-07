namespace GraduationProject.Models
{
    public class QuizQuestion
    {
        public int Id { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int Position { get; set; } 

        public int QuizId { get; set; } 
        public Quiz Quiz { get; set; } = null!;

        public ICollection<QuizAnswer> Answers { get; set; } = null!;
    }



}
