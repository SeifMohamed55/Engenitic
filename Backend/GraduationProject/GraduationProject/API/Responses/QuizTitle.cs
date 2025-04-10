using GraduationProject.Domain.DTOs;

namespace GraduationProject.API.Responses
{
    public class QuizTitleResponse : IPostitionable
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int Position { get; set; }
    }
}
