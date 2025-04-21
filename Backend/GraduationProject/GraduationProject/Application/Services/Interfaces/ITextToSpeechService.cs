namespace GraduationProject.Application.Services.Interfaces
{
    public interface ITextToSpeechService
    {
        Task<ServiceResult<byte[]>> GetAudioFromTextAsync(string text);
    }
}
