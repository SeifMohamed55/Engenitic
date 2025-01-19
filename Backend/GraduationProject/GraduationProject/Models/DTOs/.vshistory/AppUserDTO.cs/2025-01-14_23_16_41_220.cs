using Microsoft.AspNetCore.Identity;

namespace GraduationProject.Models.DTOs
{
    public class AppUserDTO
    {
        public int id { get; set; }
        public string Address { get; set; } = null!;

        public string Country { get; set; } = null!;

        public bool Banned { get; set; }

        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;


        public ICollection<string> Roles { get; set; } = new List<string>();
    }
}
