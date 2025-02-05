namespace GraduationProject.Models
{
    // TODO: Add Stages, Requirements and Tags
    public class Course
    {
        public int Id { get; set; }
        public bool hidden { get; set; }
        public string? Code { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int InstructorId { get; set; }
        public AppUser Instructor { get; set; } = null!;

        public ICollection<Quiz> Quizes { get; set; } = new List<Quiz>();
    }



}
