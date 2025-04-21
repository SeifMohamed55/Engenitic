namespace GraduationProject.Application.Services.Interfaces
{
    public interface ICloudinaryService
    {
        public static string DefaultUserImagePublicId => "uploads/images/users/default";
        public static string DefaultCourseImagePublicId => "uploads/images/courses/default";

        string GetImageUrl(string publicId, string version);
        string GetProfileImage(string publicId, string version);
        string GetPDF(string publicId, string version);

    }
}
