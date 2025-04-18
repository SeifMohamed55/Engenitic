using GraduationProject.Domain.DTOs;

namespace GraduationProject.Domain.Models
{
    public class Quiz : IEntity<int>
    {
        public Quiz() { }
        public Quiz(QuizDTO quiz)
        {
            Id = quiz.Id;
            Title = quiz.Title;
            VideoUrl = quiz.VideoUrl;
            Position = quiz.Position;
            Description = quiz.Description;
            Questions = quiz.Questions.Select(x => new QuizQuestion(x)).ToList();
        }

        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int Position { get; set; }
        public string VideoUrl { get; set; } = null!;

        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public ICollection<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();

        public void UpdateFromRequest(QuizDTO quiz)
        {
            Title = quiz.Title;
            VideoUrl = quiz.VideoUrl;
            Position = quiz.Position;
            Description = quiz.Description;
        }
    }



}
