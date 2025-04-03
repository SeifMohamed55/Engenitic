using GraduationProject.Domain.Enums;

namespace GraduationProject.Domain.Models
{
    public class FileHash
    {
        public int Id { get; set; }
        public string PublicId { get; set; } = null!;
        public CloudinaryType Type { get; set; }
        public ulong Hash { get; set; }
        public required string Version { get; set; } = null!;



        public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
        public ICollection<Course> Courses { get; set; } = new List<Course>();



        public void UpdateFromHash(FileHash hash)
        {
            PublicId = hash.PublicId;
            Hash = hash.Hash;
            Version = hash.Version;
            Type = hash.Type;
        }
    }
}
