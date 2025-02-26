namespace GraduationProject.Controllers.APIResponses
{
    public class AuthenticatedPayload
    {
        public required string Email { get; set; }
        public required string Name { get; set; }
        public required string Image {  get; set; }
        public required string UniqueId { get; set; }
    }
}
