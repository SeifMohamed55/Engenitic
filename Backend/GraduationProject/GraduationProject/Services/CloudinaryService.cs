using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using static System.Net.Mime.MediaTypeNames;

namespace GraduationProject.Services
{
    public enum CloudinaryType
    {
        UserImage,
        CourseImage,
        InstructorCV
    }

    file static class CloudinaryExtensions
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
        
        

    public interface ICloudinaryService
    {
        public string DefaultUserImagePublicId => "uploads/images/users/default";
        public string DefaultCourseImagePublicId => "uploads/images/courses/default";

        Task<string?> UploadAsync(IFormFile image, string imageName, CloudinaryType type);
        Task<string?> UploadRemoteAsync(string imageUrl, string imageName, CloudinaryType type);

        string GetImageUrl(string publicId);
        string GetProfileImage(string publicId);
        string GetPDF(string publicId);

        //string GetDefaultProfileImage();
        //string GetDefaultCourseImage();

    }

    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        public CloudinaryService(Cloudinary cloudinary) 
        {
            _cloudinary = cloudinary;
        }

        public async Task<string?> UploadAsync(IFormFile image, string imageName, CloudinaryType type)
        {
            if (!ImageHelper.IsValidImageType(image))
                return null;

            // imageName with no extension

            var typePath = type.GetTypePath();
            using var stream = image.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(imageName, image.OpenReadStream()),
                Folder = typePath,
                Overwrite = true,
                PublicId = imageName,
                Type = "authenticated",
                DisplayName = imageName
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult.PublicId;
        }

        public async Task<string?> UploadRemoteAsync(string imageUrl, string imageName, CloudinaryType type)
        {
            var typePath = type.GetTypePath();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(imageUrl),
                Folder = typePath,
                Overwrite = true,
                PublicId = $"{imageName}",
                Type = "authenticated",
                DisplayName = imageName
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult.PublicId;
        }

        public string GetImageUrl(string publicId)
        {
            return _cloudinary.Api
                .UrlImgUp
                .Secure()
                .Signed(true)
                .Type("authenticated")
                .BuildUrl(publicId);
        }


        public string GetProfileImage(string publicId)
        {
            return _cloudinary.Api.UrlImgUp
                .Transform(new Transformation()
                    .Width(200)
                    .Height(200)
                    .Crop("thumb")
                    .Quality("auto")  // Optimizes quality
                    .FetchFormat("auto"))
                .Secure()
                .Signed(true)
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


/*        public string GetDefaultProfileImage()
        {
            return _cloudinary.Api
                .UrlImgUp
                .Secure()
                .Signed(true)
                .BuildUrl("uploads/images/users/default");
        }
        public string GetDefaultCourseImage()
        {
            return _cloudinary.Api
                .UrlImgUp
                .Secure()
                .Signed(true)
                .BuildUrl("uploads/images/courses/default");
        }*/
    }
}
