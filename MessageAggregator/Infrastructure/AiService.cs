using System.Net.Http; // Keep one
using System.Net.Http.Headers; // Keep one
using System.Text; // Keep one
using System.Threading.Tasks; // Keep one
using Microsoft.Extensions.Configuration; // Keep one
// Removed duplicate usings
using MessageAggregator.Domain.Interfaces;
// Removed unused DTO using: using MessageAggregator.Domain.DTOs;
using Newtonsoft.Json;
using System;
using Domain.Models; // Added for Summary
using MessageAggregator.Infrastructure; // Added for AppDbContext
using Microsoft.EntityFrameworkCore; // Added for SaveChangesAsync etc.
using Microsoft.Extensions.Logging; // Optional: Added for logging errors

namespace Infrastructure
{
    public class AiService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _dbContext;
        private readonly ILogger<AiService> _logger;
        // Make fields nullable or ensure non-null assignment
        private readonly string _apiKey = string.Empty;
        private readonly string _endpoint = string.Empty;
        private readonly string _model = string.Empty;

        // Inject AppDbContext and ILogger
        public AiService(HttpClient httpClient, IConfiguration configuration, AppDbContext dbContext, ILogger<AiService> logger)
        {
            _httpClient = httpClient;
            _dbContext = dbContext;
            _logger = logger;
            // Add null checks or use GetValue with defaults for configuration
            _apiKey = configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException(nameof(configuration), "OpenAI:ApiKey is missing");
            _endpoint = configuration["OpenAI:Endpoint"] ?? throw new ArgumentNullException(nameof(configuration), "OpenAI:Endpoint is missing");
            _model = configuration["OpenAI:Model"] ?? throw new ArgumentNullException(nameof(configuration), "OpenAI:Model is missing");
        }

        // Change return type to Task<Summary> and add chatName parameter
        public async Task<Summary> AnalyzeAsync(IEnumerable<string> data, string chatName) // Added chatName parameter
        {
            // Сериализуем массив строк в JSON
            var jsonData = JsonConvert.SerializeObject(data);

            // Refined prompt to demand JSON only
            var refinedPrompt = $"Analyze the JSON array of strings provided below. Respond ONLY with a JSON object containing 'summary' and 'category' keys. Example format: {{\"summary\": \"brief content summary\", \"category\": \"Error\"}}. Do not include any other text or explanation. JSON Array: {jsonData}";

            var requestBody = new
            {
                model = _model,
                messages = new[]
                {
                    // Add system message to guide AI
                    new { role = "system", content = "You are an AI assistant that analyzes text data and responds ONLY with a JSON object in the format {\"summary\": \"string\", \"category\": \"string\"}." },
                    // Use the refined user prompt
                    new { role = "user", content = refinedPrompt }
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

                dynamic? result = JsonConvert.DeserializeObject(responseContent); // Use dynamic?
                string? content = result?.choices?[0]?.message?.content; // Use string?
                string summaryText = "AI response parsing error";
                string categoryName = "Unknown";

                // Парсим JSON из ответа модели
                if (!string.IsNullOrEmpty(content))
                {
                    try
                    {
                        // Use a temporary DTO or anonymous type for initial parsing
                        var aiResponse = JsonConvert.DeserializeAnonymousType(content ?? string.Empty, new { summary = "", category = "" }); // Handle potential null content
                        if (aiResponse != null && !string.IsNullOrWhiteSpace(aiResponse.summary) && !string.IsNullOrWhiteSpace(aiResponse.category))
                        {
                            summaryText = aiResponse.summary ?? "Summary missing in AI response"; // Handle potential null
                            categoryName = aiResponse.category?.Trim() ?? "Category missing in AI response"; // Handle potential null and trim

                            // Create and save the Summary object ONLY if parsing was successful
                            var summary = new Summary(
                                chatName: chatName, // Use the passed chatName
                                description: summaryText,
                                categoryName: categoryName,
                                createdAt: DateTime.UtcNow
                            );

                            try
                            {
                                _dbContext.Summaries.Add(summary);
                                await _dbContext.SaveChangesAsync();
                                _logger?.LogInformation("Successfully saved summary {SummaryId} for category {CategoryName}", summary.Id, summary.CategoryName);
                                return summary; // Return the saved summary
                            }
                            catch (DbUpdateException dbEx)
                            {
                                _logger?.LogError(dbEx, "Failed to save summary to database. Category: {CategoryName}, SummaryText: {SummaryText}", categoryName, summaryText);
                                // Throw exception to signal failure
                                throw new InvalidOperationException("Failed to save summary to the database.", dbEx);
                            }
                        }
                        else
                        {
                            // If parsing gives unexpected structure or missing fields, log and throw
                            _logger?.LogWarning("AI response content did not contain expected summary/category structure: {Content}", content);
                            throw new InvalidOperationException($"AI response content did not contain expected summary/category structure: {content}");
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        // Handle JSON parsing specific errors, log and throw
                        _logger?.LogError(jsonEx, "AI response JSON parsing error. Content: {Content}", content);
                        throw new InvalidOperationException($"AI response JSON parsing error: {content}", jsonEx);
                    }
                }
                else
                {
                     // Handle empty content case, log and throw
                    _logger?.LogWarning("AI response content was null or empty.");
                    throw new InvalidOperationException("AI response content was null or empty.");
                }

                // Code execution should not reach here if saving was successful or if an error occurred during parsing/saving
                // If it does, it implies an issue before the parsing block or an unexpected flow.
                // Throwing an exception here ensures the method doesn't implicitly return without a valid summary or explicit error.
                throw new InvalidOperationException("Reached unexpected end of AnalyzeAsync method. AI response might not have been processed correctly.");

            }
            catch (HttpRequestException httpEx)
            {
                _logger?.LogError(httpEx, "AI HTTP error occurred.");
                // Handle HTTP specific errors - Throw exception as saving failed
                 throw new InvalidOperationException("AI service failed due to HTTP error.", httpEx);
                // Or return a specific error Summary if preferred, but less ideal
                // return new Summary { Id = Guid.Empty, Description = $"AI HTTP error: {httpEx.Message}", CategoryName = "Error", CreatedAt = DateTime.UtcNow };
            }
            catch (Exception ex)
            {
                 _logger?.LogError(ex, "AI general error occurred.");
                // Handle other general errors - Throw exception
                 throw new InvalidOperationException("AI service failed due to a general error.", ex);
                // Or return a specific error Summary
                // return new Summary { Id = Guid.Empty, Description = $"AI general error: {ex.Message}", CategoryName = "Error", CreatedAt = DateTime.UtcNow };
            }
        }
    }
}
