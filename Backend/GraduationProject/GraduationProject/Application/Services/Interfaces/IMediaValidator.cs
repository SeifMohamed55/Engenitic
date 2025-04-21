namespace GraduationProject.Application.Services.Interfaces
{
    using System.Threading.Tasks;

    public interface IMediaValidator
    {
        Task<bool> ValidateAsync(string url);
        Task<MediaType> GetMediaTypeAsync(string url);
    }
}
