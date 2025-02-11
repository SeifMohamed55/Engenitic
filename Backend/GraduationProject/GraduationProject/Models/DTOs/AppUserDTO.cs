namespace GraduationProject.Models.DTOs
{
     public class AppUserDTO
        {
            public int Id { get; set; }
            public string Email { get; set; } = null!;
            public string UserName { get; set; } = null!;
            public string? PhoneNumber { get; set; } = null!;
            public string? PhoneRegionCode { get; set; } = null!;
            public string? ImageURL { get; set; } = null!;
            public List<EnrollmentDTO> Enrollments { get; set; } = new List<EnrollmentDTO>();
    }
}
