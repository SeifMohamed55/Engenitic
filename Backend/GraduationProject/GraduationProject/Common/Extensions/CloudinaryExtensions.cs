using GraduationProject.Domain.Enums;

namespace GraduationProject.Common.Extensions
{
    public static class CloudinaryExtensions
    {
        public static string GetTypePath(this CloudinaryType type)
        {
            return type switch
            {
                CloudinaryType.UserImage => "uploads/images/users",
                CloudinaryType.CourseImage => "uploads/images/courses",
                CloudinaryType.InstructorCV => "uploads/CVs",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

    }
}
