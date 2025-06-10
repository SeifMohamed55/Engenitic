using CloudinaryDotNet;
using GraduationProject.Application.Services.Interfaces;

namespace GraduationProject.Application.Services
{

    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        public CloudinaryService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }


        public string GetImageUrl(string publicId, string version)
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
                    .Version(version)
                    .BuildUrl(publicId);
        }


        public string GetProfileImage(string publicId, string version)
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
                    .Version(version)
                    .BuildUrl(publicId);
        }

        public string GetPDF(string publicId, string version)
        {
            return _cloudinary.Api
                .Url
                .ResourceType("image")
                .Secure()
                .Signed(true)
                .Type("authenticated")
                .Version(version)
                .BuildUrl(publicId);
        }
    }
}
