using GraduationProject.Models;
using GraduationProject.Models.DTOs;

namespace GraduationProject.Controllers.ApiRequest
{
    public class RegisterCourseRequest
    {
        public string? Code { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Requirements { get; set; } = null!;
        public int InstructorId { get; set; }
        public IFormFile Image { get; set; } = null!;

        public ICollection<QuizDTO> Quizes { get; set; } = new List<QuizDTO>();
    }
}
