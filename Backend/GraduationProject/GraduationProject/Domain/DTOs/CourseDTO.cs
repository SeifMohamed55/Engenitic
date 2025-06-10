using GraduationProject.Domain.Models;

namespace GraduationProject.Domain.DTOs
{
    public class CourseDTO
    {
        public CourseDTO() { Image = new FileMetadata(); }
        public CourseDTO(Course course)
        {
            Id = course.Id;
            Title = course.Title;
            Code = course.Code;
            Stages = course.Stages;
            Description = string.Join(' ', course.Description.Split(' ').Take(3).Append("..."));
            InstructorName = course.Instructor?.FullName;
            Requirements = course.Requirements;
            Image = new()
            {
                FileURL = course.FileHash.PublicId,
                Name = course.FileHash.PublicId.Split('/').LastOrDefault() ?? "",
                Hash = course.FileHash.Hash,
                Version = course.FileHash.Version
            };
        }

        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Code { get; set; }
        public string Description { get; set; } = null!;
        public string? InstructorName { get; set; } = null!;
        public string Requirements { get; set; } = null!;
        public int Stages { get; set; }
        public FileMetadata Image { get; set; }

    }
}
