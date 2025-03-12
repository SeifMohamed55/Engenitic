namespace GraduationProject.API.Requests
{
    public class RoleRequest
    {
        public int OldId { get; set; } = 0;
        public string? OldName { get; set; }
        public string? NewName { get; set; }
    }
}
