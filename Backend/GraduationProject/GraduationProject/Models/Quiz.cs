using GraduationProject.Models.DTOs;

namespace GraduationProject.Models
{
    public class Quiz
    {
        public Quiz() { }
        public Quiz(QuizDTO quiz)
        {
            Title = quiz.Title;
            VideoUrl = quiz.VideoUrl;
            Position = quiz.Position;
            Questions = quiz.Questions.Select(x => new QuizQuestion(x)).ToList();
        }

        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int Position { get; set; }
        public string VideoUrl { get; set; } = null!;

        public int CourseId { get; set; } 
        public Course Course { get; set; } = null!;

        public ICollection<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();
    }



}
