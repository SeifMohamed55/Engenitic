using GraduationProject.Domain.DTOs;
using Microsoft.AspNetCore.Identity;

namespace GraduationProject.Domain.Models
{
    public class AppUser : IdentityUser<int>, IEntity<int>
    {
        [ProtectedPersonalData]
        public override string Email { get; set; } = null!;
        public string? PhoneRegionCode { get; set; }
        public bool Banned { get; set; }
        public string FullName { get; set; } = null!;
        public bool IsExternal { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<Role> Roles { get; set; } = new List<Role>();
        public ICollection<Course> Courses { get; set; } = new List<Course>(); // Course Maker
        public ICollection<UserEnrollment> Enrollments { get; set; } = new List<UserEnrollment>(); // Student
        public ICollection<FileHash> FileHashes { get; set; } = new List<FileHash>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();

        public void UpdateFromDTO(AppUserDTO dto)
        { 
            Id = dto.Id;
            Email = dto.Email;
            PhoneNumber = dto.PhoneNumber;
            PhoneRegionCode = dto.PhoneRegionCode;
            FullName = dto.UserName;
        }
    }
}