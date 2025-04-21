namespace GraduationProject.Application.Services.Interfaces
{
    using System.Threading.Tasks;

    public interface IGmailServiceHelper
    {
        Task SendEmailAsync(string to, string subject, string body);

    }


}
