using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using GraduationProject.Common.Extensions;
using GraduationProject.Domain.Enums;
using GraduationProject.Domain.Models;
using Microsoft.AspNetCore.StaticFiles;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace GraduationProject.Application.Services
{

    public interface IUploadingService
    {
        Task<FileHash?> UploadImageAsync(string imageUrl, string imageName, CloudinaryType type);
        Task<FileHash?> UploadImageAsync(Stream stream, string imageName, CloudinaryType type);
    }

    public class UploadingService : IUploadingService
    {

        private readonly Cloudinary _cloudinary;
        private readonly IHashingService _hashingService;

        public UploadingService(Cloudinary cloudinary, IHashingService hashingService)
        {
            _cloudinary = cloudinary;
            _hashingService = hashingService;
        }

        public static bool IsValidImageType([NotNullWhen(true)] IFormFile? image, long maxSize = 2 * 1024 * 1024)
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

                long maxFileSize = maxSize; // 2MB
                if (image.Length > maxFileSize)
                {
                    return false;
                }
                return true;
            }
        }

        public async Task<FileHash?> UploadImageAsync(Stream stream, string imageName, CloudinaryType type)
        {
            var publicId = await UploadAsync(stream, imageName, type);
            if (publicId == null)
                return null;

            return new FileHash
            {
                Type = type,
                PublicId = publicId,
                Hash = await _hashingService.HashWithxxHash(stream)
            };
        }

        private async Task<string?> UploadAsync(Stream image, string imageName, CloudinaryType type)
        {

            var typePath = type.GetTypePath();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(imageName, image),
                Folder = typePath,
                Overwrite = true,
                PublicId = imageName,
                Type = "authenticated",
                DisplayName = imageName
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult.PublicId;
        }

        public async Task<FileHash?> UploadImageAsync(string imageUrl, string imageName, CloudinaryType type)
        {
            var publicId = await UploadAsync(imageUrl, imageName, type);
            if (publicId == null)
                return null;

            await using var stream = await GetFileStreamAsync(publicId);
            if (stream == null)
                throw new NoNullAllowedException("Uploaded file is null");


            return new FileHash
            {
                Type = type,
                PublicId = publicId,
                Hash = await _hashingService.HashWithxxHash(stream)
            };
        }

        private async Task<string?> UploadAsync(string imageUrl, string imageName, CloudinaryType type)
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

        private async Task<Stream?> GetFileStreamAsync(string publicId)
        {
            var resource = await _cloudinary.GetResourceAsync(new GetResourceParams(publicId));

            if (resource == null || string.IsNullOrEmpty(resource.Url))
            {
                return null;
            }

            using var httpClient = new HttpClient();
            // Download the file
            var response = await httpClient.GetAsync(resource.Url);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadAsStreamAsync();

        }


    }
}
