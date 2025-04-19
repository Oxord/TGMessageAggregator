using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MessageAggregator.Domain.Interfaces;
using MessageAggregator.Domain.DTOs;
using Newtonsoft.Json;
using System;
using Domain.Models; // Added for Category model

namespace Infrastructure
{
    public class AiService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly ICategoryRepository _categoryRepository; // Added repository
        private readonly string _apiKey;
        private readonly string _endpoint;
        private readonly string _model;

        // Inject ICategoryRepository
        public AiService(HttpClient httpClient, IConfiguration configuration, ICategoryRepository categoryRepository)
        {
            _httpClient = httpClient;
            _categoryRepository = categoryRepository; // Assign injected repository
            _apiKey = configuration["OpenAI:ApiKey"];
            _endpoint = configuration["OpenAI:Endpoint"];
            _model = configuration["OpenAI:Model"];
        }

        // Change parameter type to IEnumerable<string>
        public async Task<AiAnalysisResultDto> AnalyzeAsync(IEnumerable<string> data)
        {
            // Сериализуем массив строк в JSON
            var jsonData = JsonConvert.SerializeObject(data);

            // Формируем prompt для OpenAI, вставляя JSON-строку
            var prompt = $"Проанализируй следующий массив строк в формате JSON и определи общее краткое содержание и категорию (например, 'Error', 'Info', 'Other') для всего массива. Ответь в формате: {{\"summary\": \"...\", \"category\": \"...\"}}. Массив строк: {jsonData}";

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
                string summaryText = "AI response parsing error";
                string categoryName = "Unknown";
                int? categoryId = null;

                // Парсим JSON из ответа модели
                if (!string.IsNullOrEmpty(content))
                {
                    try
                    {
                        // Use a temporary DTO or anonymous type for initial parsing
                        var aiResponse = JsonConvert.DeserializeAnonymousType(content, new { summary = "", category = "" });
                        if (aiResponse != null && !string.IsNullOrWhiteSpace(aiResponse.summary) && !string.IsNullOrWhiteSpace(aiResponse.category))
                        {
                            summaryText = aiResponse.summary;
                            categoryName = aiResponse.category.Trim(); // Trim whitespace

                            // Find category by name using the repository
                            Category? category = await _categoryRepository.GetByNameAsync(categoryName);
                            categoryId = category?.Id; // Assign ID if found, otherwise null
                        }
                        else
                        {
                            // If parsing gives unexpected structure, use the raw content
                            summaryText = content;
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        // Handle JSON parsing specific errors, maybe log jsonEx
                        summaryText = $"AI response JSON parsing error: {content}. Error message: {jsonEx.Message}";
                        categoryName = "Error";
                        // Optionally find the 'Error' category ID here if needed
                        Category? errorCategory = await _categoryRepository.GetByNameAsync("Error");
                        categoryId = errorCategory?.Id;
                    }
                }

                return new AiAnalysisResultDto
                {
                    SummaryText = summaryText,
                    CategoryId = categoryId,
                    OriginalCategoryName = categoryName
                };
            }
            catch (HttpRequestException httpEx)
            {
                // Handle HTTP specific errors
                Category? errorCategory = await _categoryRepository.GetByNameAsync("Error");
                return new AiAnalysisResultDto
                {
                    SummaryText = $"AI HTTP error: {httpEx.Message}",
                    CategoryId = errorCategory?.Id,
                    OriginalCategoryName = "Error"
                };
            }
            catch (Exception ex)
            {
                // Handle other general errors
                Category? errorCategory = await _categoryRepository.GetByNameAsync("Error");
                return new AiAnalysisResultDto
                {
                    SummaryText = $"AI general error: {ex.Message}",
                    CategoryId = errorCategory?.Id,
                    OriginalCategoryName = "Error"
                };
            }
        }
    }
}
