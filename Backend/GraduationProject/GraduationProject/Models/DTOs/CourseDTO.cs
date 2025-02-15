namespace GraduationProject.Models.DTOs
{
    public class CourseDTO
    {
        public CourseDTO() { }
        public CourseDTO(Course course)
        {
            Id = course.Id;
            Title = course.Title;
            Code = course.Code;
            Description = course.Description;
            InstructorName = course.Instructor.FullName;
            InstructorEmail = course.Instructor.Email;
            InstructorPhone = course.Instructor.PhoneNumber;
            Requirements = course.Requirements;
            Image = new() { ImageURL = "https://localhost/api/users/image", Name = course.ImageUrl};
        }

        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Code { get; set; }
        public string Description { get; set; } = null!;
        public string InstructorName { get; set; } = null!;
        public string InstructorEmail { get; set; } = null!;
        public string? InstructorPhone { get; set; }
        public string Requirements { get; set; } = null!;
        public ImageMetadata? Image { get; set; }

    }
}
