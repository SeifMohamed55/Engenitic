using GraduationProject.API.Requests;
using GraduationProject.API.Responses;
using GraduationProject.Application.Services.Interfaces;
using GraduationProject.StartupConfigurations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace GraduationProject.Application.Services.HttpClientServices
{
    public class GrammarCorrectionService : IGrammarCorrectionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly JsonSerializerOptions _jsonOptions;

        public GrammarCorrectionService(
            HttpClient httpClient,
            IOptions<ApiKeyOption> options,
            IOptions<JsonOptions> jsonOptions
            )
        {
            _httpClient = httpClient;
            _apiKey = options.Value.ApiKey;
            _jsonOptions = jsonOptions.Value.JsonSerializerOptions;
        }
        public async Task<ServiceResult<GrammarCorrectionResponse>> CorrectGrammar(GrammarCorrectionRequest request)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                return ServiceResult<GrammarCorrectionResponse>.Failure("Api key not configured", System.Net.HttpStatusCode.ServiceUnavailable);
            }
            try
            {
                _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("predict", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<GrammarCorrectionResponse>();
                if (result == null)
                    throw new JsonException("Failed to retrieve the response.");
                return ServiceResult<GrammarCorrectionResponse>.Success(result, "Grammar is corrected successfully.", System.Net.HttpStatusCode.OK);
            }
            catch (HttpRequestException e)
            {
                return ServiceResult<GrammarCorrectionResponse>.Failure(e.Message, System.Net.HttpStatusCode.BadRequest);
            }
            catch (JsonException e)
            {
                return ServiceResult<GrammarCorrectionResponse>.Failure(e.Message, System.Net.HttpStatusCode.BadRequest);
            }
            catch
            {
                return ServiceResult<GrammarCorrectionResponse>.Failure("An error Occured", System.Net.HttpStatusCode.BadRequest);
            }
        }
    }
}
