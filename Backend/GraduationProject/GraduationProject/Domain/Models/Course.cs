using GraduationProject.API.Requests;

namespace GraduationProject.Domain.Models
{
    // TODO: Add Stages, Requirements and Tags
    public class Course : IEntity<int>
    {
        public Course() { }
        public Course(RegisterCourseRequest course, List<Tag> tags, FileHash hash)
        {
            Code = course.Code;
            Title = course.Title;
            Description = course.Description;
            Requirements = course.Requirements;
            InstructorId = course.InstructorId;
            Quizes = course.Quizes.Select(x => new Quiz(x)).ToList();
            Stages = Quizes.Count;
            Tags = tags;
            FileHash = hash;
        }


        public int Id { get; set; }
        public bool hidden { get; set; }
        public string? Code { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Requirements { get; set; } = null!;
        public int Stages { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public double AverageRating { get; set; }

        public int InstructorId { get; set; }
        public AppUser Instructor { get; set; } = null!;

        public int HashId { get; set; }
        public FileHash FileHash { get; set; } = null!;

        public ICollection<Quiz> Quizes { get; set; } = new List<Quiz>();
        public ICollection<UserEnrollment> Enrollments { get; set; } = new List<UserEnrollment>();
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();

        public void UpdateFromRequest(EditCourseRequest course)
        {
            Code = course.Code;
            Title = course.Title;
            Description = course.Description;
            Requirements = course.Requirements;
            Stages = Quizes.Count;
        }

    }



}
