using GraduationProject.Application.Services.Interfaces;
using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Enums;
using GraduationProject.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Common.Extensions
{
    public static class ReviewExtensions
    {
        public static IQueryable<ReviewDTO> DTOProjection(this IQueryable<Review> query)
        {
            return query
                .Select(x => new ReviewDTO
                {
                    ReviewId = x.Id,
                    Content = x.Content,
                    UpdatedAt = x.UpdatedAt,
                    UserId = x.UserId,
                    CourseId = x.CourseId,
                    FullName = x.User.FullName,
                    ImageMetadata = new()
                    {
                        ImageURL = x.User.FileHashes
                              .Where(z => z.Type == CloudinaryType.UserImage)
                              .Select(z => z.PublicId)
                              .FirstOrDefault() ?? ICloudinaryService.DefaultUserImagePublicId,
                        Name = "user image",
                        Hash = x.User.FileHashes
                              .Where(z => z.Type == CloudinaryType.UserImage)
                              .Select(z => z.Hash)
                              .FirstOrDefault(),
                        Version = x.User.FileHashes
                               .Where(z => z.Type == CloudinaryType.UserImage)
                              .Select(z => z.Version)
                              .FirstOrDefault() ?? "1"
                    },
                    Rating = x.Rating,
                })
                .OrderBy(x=> x.Rating);
        }
    }
}
