namespace GraduationProject.Models
{
    public class QuizAnswer // each question has 1 answer
    {
        public int Id { get; set; }
        public string AnswerText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }



}
