namespace GraduationProject.Models
{
    public class Quiz
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int Position { get; set; } 
        public int CourseId { get; set; } 
        public Course Course { get; set; } = null!;

        public ICollection<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();
    }



}
