using GraduationProject.Application.Services.Interfaces;
using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Enums;
using GraduationProject.Domain.Models;

namespace GraduationProject.Common.Extensions
{
    public static class AppUserExtension
    {
        public static IQueryable<AppUserDTO> DTOProjection(this IQueryable<AppUser> query)
        {
            return query
                .Select(x => new AppUserDTO
                {
                    Id = x.Id,
                    Email = x.Email,
                    PhoneNumber = x.PhoneNumber,
                    PhoneRegionCode = x.PhoneRegionCode,
                    UserName = x.FullName,
                    Banned = x.Banned,
                    IsExternal = x.IsExternal,
                    IsEmailConfirmed = x.EmailConfirmed,
                    CreatedAt = x.CreatedAt,
                    Roles = x.Roles
                        .OrderBy(x=> x.Id)
                        .Select(r => r.Name)
                        .ToList(),
                    Image = new()
                    {
                        FileURL = x.FileHashes
                              .Where(z => z.Type == CloudinaryType.UserImage)
                              .Select(z => z.PublicId)
                              .FirstOrDefault() ?? ICloudinaryService.DefaultUserImagePublicId,
                        Name = "user image",
                        Hash = x.FileHashes
                              .Where(z => z.Type == CloudinaryType.UserImage)
                              .Select(z => z.Hash)
                              .FirstOrDefault(),
                        Version = x.FileHashes
                               .Where(z => z.Type == CloudinaryType.UserImage)
                              .Select(z => z.Version)
                              .FirstOrDefault() ?? "1"
                    },
                    Cv = new()
                    {
                        FileURL = x.FileHashes
                              .Where(z => z.Type == CloudinaryType.InstructorCV)
                              .Select(z => z.PublicId)
                              .FirstOrDefault() ?? "",
                        Name = "User Cv",
                        Hash = x.FileHashes
                              .Where(z => z.Type == CloudinaryType.InstructorCV)
                              .Select(z => z.Hash)
                              .FirstOrDefault(),
                        Version = x.FileHashes
                               .Where(z => z.Type == CloudinaryType.InstructorCV)
                              .Select(z => z.Version)
                              .FirstOrDefault() ?? "1"
                    }
                });
        }
    }
}
