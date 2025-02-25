namespace GraduationProject.Controllers.ApiRequest
{
    public class UpdateEmailRequest
    {
        public int Id { get; set; }
        public string NewEmail { get; set; } = null!;
    }
}
