using System.Net.Http.Headers;
using System.Text;
using MessageAggregator.Domain.DTOs;
using MessageAggregator.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace MessageAggregator.Infrastructure;

public class AiService(HttpClient httpClient, IConfiguration configuration) : IAiService
{
    private readonly string _apiKey = configuration["OpenAI:ApiKey"]!;
    private readonly string _endpoint = configuration["OpenAI:Endpoint"]!;
    private readonly string _model = configuration["OpenAI:Model"]!;

    public async Task<List<AiAnalysisResultDto>> AnalyzeAsync(List<string> data, List<string> intends)
    {
        try
        {
            string content = await SendPrompt(data, intends);

            try
            {
                List<AiAnalysisResultDto>? aiResponses =
                    JsonConvert.DeserializeObject<List<AiAnalysisResultDto>>(content);
                if (aiResponses is not { Count: > 0 })
                    return
                    [
                        new AiAnalysisResultDto
                        {
                            Summary = content,
                            Intend = "Unknown"
                        },
                    ];

                foreach (AiAnalysisResultDto item in aiResponses)
                {
                    item.Intend = item.Intend.Trim();
                }

                return aiResponses;
            }
            catch (JsonException jsonEx)
            {
                // Handle JSON parsing specific errors, maybe log jsonEx
                return
                [
                    new AiAnalysisResultDto
                    {
                        Summary = $"AI response JSON parsing error: {content}. Error message: {jsonEx.Message}",
                        Intend = "Error"
                    },
                ];
            }
        }
        catch (HttpRequestException httpEx)
        {
            // Handle HTTP specific errors
            return
            [
                new AiAnalysisResultDto
                {
                    Summary = $"AI HTTP error: {httpEx.Message}",
                    Intend = "Error"
                },
            ];
        }
        catch (Exception ex)
        {
            // Handle other general errors
            return
            [
                new AiAnalysisResultDto
                {
                    Summary = $"AI general error: {ex.Message}",
                    Intend = "Error"
                },
            ];
        }
    }

    private async Task<string> SendPrompt(List<string> data, List<string> intends)
    {
        string jsonData = JsonConvert.SerializeObject(data);

        string prompt =
            $"Представь, что ты - человек, которому надо мониторить чаты и вычленять из них важную информацию для бизнеса компании. " +
            $"Проанализируй все сообщения из чата Telegram в виде массива строк в формате JSON и составь своими словами небольшую сводку " +
            $"по следующим категориям ({string.Join(", ", intends)}). На каждую категорию должна быть своя выжимка. " +
            $"Ответь без лишних слов чётко в формате JSON: [{{\"summary\": \"...\", \"intend\": \"...\"}}], " +
            $"где каждый элемент массива - одна из категорий. Массив строк: {jsonData}";

        object requestBody = new
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

        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();

        // Пример структуры ответа OpenAI
        // {
        //   "choices": [
        //     {
        //       "message": {
        //         "content": "[{\"summary\": \"...\", \"intend\": \"...\"}, ...]"
        //       }
        //     }
        //   ]
        // }

        dynamic? result = JsonConvert.DeserializeObject(responseContent);
        string? content = result?.choices?[0]?.message?.content;
        if (content == null)
        {
            throw new InvalidOperationException("Prompt was empty");
        }

        return content.Substring(7, content.Length - 10);
    }
}