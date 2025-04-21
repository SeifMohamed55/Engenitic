using GraduationProject.API.Responses;

namespace GraduationProject.Application.Services.Interfaces
{
    public interface IVqaService
    {
        Task<ServiceResult<VqaResponse>> GetAnswerAsync(IFormFile image, string question);
    }
}
