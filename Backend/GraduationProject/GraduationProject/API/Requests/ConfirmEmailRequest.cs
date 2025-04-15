namespace GraduationProject.API.Requests
{
    public class ConfirmEmailRequest
    {
        public string UserId { get; set; } = null!;
        public string NewEmail { get; set; } = null!;
        public string Token { get; set; } = null!;
    }

}
