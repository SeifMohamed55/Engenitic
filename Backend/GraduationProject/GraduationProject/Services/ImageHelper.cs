using Microsoft.AspNetCore.StaticFiles;

namespace GraduationProject.Services
{
    public static class ImageHelper
    {
        public static bool IsValidImageType(IFormFile? image)
        {
            if (image == null || image.Length == 0)
                return false;
            else
            {
                var allowedExtensions = new HashSet<string>
                {
                    ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".tiff"
                };


                var provider = new FileExtensionContentTypeProvider();
                if (!provider.TryGetContentType(image.FileName, out var contentType))
                {
                    return false;
                }
                var allowedMimeTypes = new HashSet<string>
                {
                    "image/jpeg", "image/png", "image/gif", "image/bmp",
                    "image/webp", "image/tiff"
                };

                var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension) || !allowedMimeTypes.Contains(contentType))
                {
                    return false;
                }

                long maxFileSize = 2 * 1024 * 1024; // 2MB
                if (image.Length > maxFileSize)
                {
                    return false;
                }
                return true;
            }
        }


        public static string GetImageType(string fileExtension)
        {
            return fileExtension switch
            {
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                _ => "application/octet-stream", // default MIME type if unknown
            };
        }

        public static string GetImageExtenstion(string MIMEtype)
        {
            return MIMEtype switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/gif" => ".gif",
                "image/bmp" => ".bmp",
                "image/webp" => ".webp",
                _ => "application/octet-stream", // default MIME type if unknown
            };
        }
    }
}
