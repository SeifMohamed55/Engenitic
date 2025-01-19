namespace SpringBootCloneApp.Controllers.APIResponses
{
    public class AuthenticatedPayload
    {
        public required string Id { get; set; }
        public required string Email { get; set; }
        public required string GivenName { get; set; }
        public required string FamilyName { get; set; }
    }
}
