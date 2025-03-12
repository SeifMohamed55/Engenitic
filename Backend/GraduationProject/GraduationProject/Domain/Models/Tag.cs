using GraduationProject.Domain.DTOs;

namespace GraduationProject.Domain.Models
{
    public class Tag
    {
        public Tag(TagDTO dto)
        {
            Id = dto.Id;
            Value = dto.Value;
        }

        public Tag(string value)
        {
            Value = value;
        }

        public int Id { get; set; }
        public string Value { get; set; } = null!;

        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
