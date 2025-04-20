using GraduationProject.StartupConfigurations;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Utilities;

namespace GraduationProject.Application.Services
{

    public interface ITextToSpeechService
    {
        Task<ServiceResult<byte[]>> GetAudioFromTextAsync(string text);
    }

    public class TextToSpeechService : ITextToSpeechService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public TextToSpeechService(HttpClient httpClient, IOptions<ApiKeyOption> options)
        {
            _httpClient = httpClient;
            _apiKey = options.Value.ApiKey;
        }

        public async Task<ServiceResult<byte[]>> GetAudioFromTextAsync(string text)
        {
            _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);
            var response = await _httpClient.PostAsJsonAsync("/text-to-speech/", new { text });

            if (response.IsSuccessStatusCode)
            {
                // Read the byte array of the audio file from the response
                var bytes = await response.Content.ReadAsByteArrayAsync();
                return ServiceResult<byte[]>.Success(bytes);
            }
            else
            {
                return ServiceResult<byte[]>.Failure("Error generating speech: " + response.ReasonPhrase);
            }
        }
    }
}
