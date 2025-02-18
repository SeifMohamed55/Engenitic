using GraduationProject.Models;
using GraduationProject.Models.DTOs;
using GraduationProject.ValidationAttributes;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GraduationProject.Controllers.ApiRequest
{
    public class RegisterCourseRequest
    {
        public string? Code { get; set; }

        [Required]
        [StringLength(100)]
        [NotEmptyOrWhiteSpace]
        public string Title { get; set; } = null!;

        [Required]
        [NotEmptyOrWhiteSpace]
        public string Description { get; set; } = null!;

        [Required]
        [StringLength(300)]
        [NotEmptyOrWhiteSpace]
        public string Requirements { get; set; } = null!;

        [Required]
        public int InstructorId { get; set; }
        [Required]
        public IFormFile Image { get; set; } = null!;

        [Required]
        public string TagsStr { get; set; } = null!; 
        public List<TagDTO> Tags { get; set; } = new List<TagDTO>();

        [Required]
        public string QuizesStr { get; set; } = null!;

        [Required]
        [UniquePostition]
        [NotEmptyCollection]
        public ICollection<QuizDTO> Quizes { get; set; } = new List<QuizDTO>();
    }
}
