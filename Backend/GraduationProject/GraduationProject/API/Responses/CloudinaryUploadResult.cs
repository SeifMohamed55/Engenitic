using CloudinaryDotNet.Actions;

namespace GraduationProject.API.Responses
{
    public class CloudinaryUploadResult
    {
        public CloudinaryUploadResult(ImageUploadResult uploadResult)
        {
            PublicId = uploadResult.PublicId;
            Version = uploadResult.Version;
        }

        public CloudinaryUploadResult(RawUploadResult uploadResult)
        {
            PublicId = uploadResult.PublicId;
            Version = uploadResult.Version;
        }

        public string PublicId { get; set; }
        public string Version { get; set; }

    }
}
