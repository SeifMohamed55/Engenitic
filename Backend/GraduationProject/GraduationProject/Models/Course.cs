namespace GraduationProject.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;

        public int InstructorId { get; set; }
        public AppUser Instructor { get; set; } = null!;
    }
}
