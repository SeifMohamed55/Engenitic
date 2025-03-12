using GraduationProject.API.Responses;
using GraduationProject.StartupConfigurations;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;

namespace GraduationProject.Application.Services
{

    public interface IVqaService
    {
        Task<ServiceResult<VqaResponse>> GetAnswerAsync(IFormFile image, string question);
    }

    public class VqaService : IVqaService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public VqaService(HttpClient httpClient, IOptions<VqaApiKeyOption> options)
        {
            _httpClient = httpClient;
            _apiKey = options.Value.ApiKey;
        }

        public async Task<ServiceResult<VqaResponse>> GetAnswerAsync(IFormFile image, string question)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                return ServiceResult<VqaResponse>.Failure("Api key not configured");
            }
            _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);

            using var content = new MultipartFormDataContent();

            using var stream = image.OpenReadStream();

            var imageContent = new StreamContent(stream);
            imageContent.Headers.ContentType = new MediaTypeHeaderValue(image.ContentType);
            content.Add(imageContent, "image", image.FileName);
            content.Add(new StringContent(question), "question");
            try
            {
                var response = await _httpClient.PostAsync("predict", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<VqaResponse>();
                if (result == null)
                    throw new JsonException("Failed to retrieve the response.");
                return ServiceResult<VqaResponse>.Success(result);
            }
            catch(HttpRequestException e)
            {
                return ServiceResult<VqaResponse>.Failure(e.Message);
            }
            catch (JsonException e)
            {
                return ServiceResult<VqaResponse>.Failure(e.Message);
            }
            catch
            {
                return ServiceResult<VqaResponse>.Failure("An error Occured");
            }

        }
    }
}
