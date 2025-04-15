namespace GraduationProject.API.Requests
{
    public class DeviceInfo
    {
        public Guid DeviceId { get; set; }
        public string IpAddress { get; set; } = null!;
        public string UserAgent { get; set; } = null!;
    }
}
