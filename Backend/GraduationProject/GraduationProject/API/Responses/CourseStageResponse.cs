using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace GraduationProject.API.Responses
{
    public class CourseStageResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public int Position { get; set; }
        public string VideoUrl { get; set; } = null!;
        public List<QuestionDTO> Questions { get; set; } = new List<QuestionDTO>();
        public float Progress { get; set; }

    }
}
