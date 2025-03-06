using AngleSharp.Text;

namespace GraduationProject.Models.DTOs
{
    public class CourseDTO
    {
        public CourseDTO() { Image = new ImageMetadata(); }
        public CourseDTO(Course course)
        {
            Id = course.Id;
            Title = course.Title;
            Code = course.Code;
            Stages = course.Stages;
            Description = string.Join(' ', course.Description.Split(' ').Take(3).Append("..."));
            InstructorName = course.Instructor?.FullName;
            Requirements = course.Requirements;
            Image = new() { ImageURL = course.ImageUrl, Name = course.ImageUrl.Split('/').LastOrDefault() ?? ""};
        }

        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Code { get; set; }
        public string Description { get; set; } = null!;
        public string? InstructorName { get; set; } = null!;
        public string Requirements { get; set; } = null!;
        public int Stages { get; set; }
        public ImageMetadata Image { get; set; }

    }
}
