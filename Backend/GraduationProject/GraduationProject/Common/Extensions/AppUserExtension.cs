using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Enums;
using GraduationProject.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Common.Extensions
{
    public static class AppUserExtension
    {
        public static IQueryable<AppUserDTO> DTOProjection(this IQueryable<AppUser> query)
        {
            return query
                .Include(x => x.FileHashes)
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
                    Image = new()
                    {
                        ImageURL = x.FileHashes
                              .Where(z => z.Type == CloudinaryType.UserImage)
                              .Select(z => z.PublicId)
                              .FirstOrDefault() ?? "",
                        Name = "",
                        Hash = x.FileHashes
                              .Where(z => z.Type == CloudinaryType.UserImage)
                              .Select(z => z.Hash)
                              .FirstOrDefault(),
                        Version = x.FileHashes
                               .Where(z => z.Type == CloudinaryType.UserImage)
                              .Select(z => z.Version)
                              .FirstOrDefault() ?? ""
                    }
                });
        }
    }
}
