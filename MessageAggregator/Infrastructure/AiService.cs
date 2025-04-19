using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MessageAggregator.Domain.Interfaces;
using MessageAggregator.Domain.DTOs;
using Newtonsoft.Json;
using System;

namespace Infrastructure
{
    public class AiService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _endpoint;
        private readonly string _model;

        public AiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["OpenAI:ApiKey"];
            _endpoint = configuration["OpenAI:Endpoint"];
            _model = configuration["OpenAI:Model"];
        }

        public async Task<CategorySummaryDto> AnalyzeAsync(string data)
        {
            // Формируем prompt для OpenAI
            var prompt = $"Проанализируй следующий текст и определи его краткое содержание и категорию (например, 'Error', 'Info', 'Other'). Ответь в формате: {{\"summary\": \"...\", \"category\": \"...\"}}. Текст: {data}";

            var requestBody = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var requestJson = JsonConvert.SerializeObject(requestBody);

            var request = new HttpRequestMessage(HttpMethod.Post, _endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            request.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();

                // Пример структуры ответа OpenAI
                // {
                //   "choices": [
                //     {
                //       "message": {
                //         "content": "{\"summary\": \"...\", \"category\": \"...\"}"
                //       }
                //     }
                //   ]
                // }

                dynamic result = JsonConvert.DeserializeObject(responseContent);
                string content = result?.choices?[0]?.message?.content;

                // Парсим JSON из ответа модели
                if (!string.IsNullOrEmpty(content))
                {
                    var aiResult = JsonConvert.DeserializeObject<CategorySummaryDto>(content);
                    if (aiResult != null)
                        return aiResult;
                }

                // Fallback: если не удалось распарсить, возвращаем весь ответ как summary
                return new CategorySummaryDto
                {
                    Summary = content ?? "AI response parsing error",
                    Category = "Unknown"
                };
            }
            catch (Exception ex)
            {
                // В случае ошибки возвращаем информацию об ошибке
                return new CategorySummaryDto
                {
                    Summary = $"AI error: {ex.Message}",
                    Category = "Error"
                };
            }
        }
    }
}
