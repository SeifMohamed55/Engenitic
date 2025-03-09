using GraduationProject.Services;

namespace GraduationProject.Models.DTOs
{
    public class ImageMetadata
    {

        public string Name { get; set; } = null!;
        public string ImageURL { get; set; } = null!;
        public ulong Hash { get; set; }
    }
}
