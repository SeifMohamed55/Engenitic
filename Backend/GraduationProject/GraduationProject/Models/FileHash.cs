using GraduationProject.Services;

namespace GraduationProject.Models
{
    public class FileHash
    {
        public int Id { get; set; }
        public string PublicId { get; set; } = null!;
        public CloudinaryType Type { get; set; }
        public ulong Hash { get; set; }

        public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
