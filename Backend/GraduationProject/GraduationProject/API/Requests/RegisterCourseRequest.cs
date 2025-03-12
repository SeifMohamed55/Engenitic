using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.ValidationAttributes;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GraduationProject.API.Requests
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

        /*        [Required]
                public string TagsStr { get; set; } = null!; */

        [Required]
        [BindProperty(BinderType = typeof(JsonModelBinder))]
        public List<TagDTO> Tags { get; set; } = new List<TagDTO>();

        /* [Required]
         public string QuizesStr { get; set; } = null!;*/

        [Required]
        [UniquePostition]
        [NotEmptyCollection]
        [BindProperty(BinderType = typeof(JsonModelBinder))]
        public ICollection<QuizDTO> Quizes { get; set; } = new List<QuizDTO>();


    }
}
