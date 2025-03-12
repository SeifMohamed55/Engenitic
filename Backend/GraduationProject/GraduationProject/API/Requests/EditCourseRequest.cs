using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.ValidationAttributes;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GraduationProject.API.Requests
{
    public class EditCourseRequest
    {
        [Required]
        public int Id { get; set; }

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
        [BindProperty(BinderType = typeof(JsonModelBinder))]
        public List<TagDTO> Tags { get; set; } = new List<TagDTO>();


        [Required]
        [UniquePostition]
        [NotEmptyCollection]
        [BindProperty(BinderType = typeof(JsonModelBinder))]
        public ICollection<QuizDTO> Quizes { get; set; } = new List<QuizDTO>();

    }


    public class DeleteCourseRequest
    {
        [Required]
        public int CourseId { get; set; }

        [Required]
        public int InstructorId { get; set; }

    }
}
