using System.ComponentModel.DataAnnotations;

namespace GraduationProject.Controllers.ApiRequest
{
    public class RoleRequest
    {
        public int OldId { get; set; } = 0;
        public string? OldName { get; set; }
        public string? NewName { get; set; }
    }
}
