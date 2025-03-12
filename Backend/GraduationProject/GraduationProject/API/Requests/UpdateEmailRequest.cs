namespace GraduationProject.API.Requests
{
    public class UpdateEmailRequest
    {
        public int Id { get; set; }
        public string NewEmail { get; set; } = null!;
    }
}
