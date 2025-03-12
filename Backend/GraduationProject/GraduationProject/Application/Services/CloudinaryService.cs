using CloudinaryDotNet;

namespace GraduationProject.Application.Services
{



    public interface ICloudinaryService
    {
        public static string DefaultUserImagePublicId => "uploads/images/users/default";
        public static string DefaultCourseImagePublicId => "uploads/images/courses/default";

        string GetImageUrl(string publicId);
        string GetProfileImage(string publicId);
        string GetPDF(string publicId);

    }

    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        public CloudinaryService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }


        public string GetImageUrl(string publicId)
        {
            var link = _cloudinary.Api
                .UrlImgUp
                .Secure();

            if (publicId == ICloudinaryService.DefaultUserImagePublicId ||
                publicId == ICloudinaryService.DefaultCourseImagePublicId)
                return link.BuildUrl(publicId);

            else
                return link.Signed(true)
                    .Type("authenticated")
                    .BuildUrl(publicId);
        }


        public string GetProfileImage(string publicId)
        {
            var link = _cloudinary.Api.UrlImgUp
                .Transform(new Transformation()
                    .Width(200)
                    .Height(200)
                    .Crop("thumb")
                    .Quality("auto")  // Optimizes quality
                    .FetchFormat("auto"))
                .Secure();

            if (publicId == ICloudinaryService.DefaultUserImagePublicId)
                return link.BuildUrl(publicId);
            else
                return link.Signed(true)
                    .Type("authenticated")
                    .BuildUrl(publicId);
        }

        public string GetPDF(string publicId)
        {
            return _cloudinary.Api
                .Url
                .ResourceType("raw")
                .Secure()
                .Signed(true)
                .Type("authenticated")
                .BuildUrl(publicId);
        }
    }
}
