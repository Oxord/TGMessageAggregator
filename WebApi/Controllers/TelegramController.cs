using System.ComponentModel.DataAnnotations;
using MessageAggregator.Application.Service;
using MessageAggregator.Domain.DTOs;
using MessageAggregator.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using TL;

namespace WebApi.Controllers;

[ApiController]
[Route("/api/telegram")]
public class TelegramController(TelegramService telegramService, IAiService openAiService) : ControllerBase
{
    [HttpGet("/chats")]
    public async Task<ActionResult<List<ChatBase>>> GetAllChats(
        [Required] [FromQuery] long chatId,
        [FromQuery] int count = 100,
        [FromQuery] string? verificationCode = null
    )
    {
        List<string> messages = await telegramService.GetMessagesAsync(chatId, count, verificationCode);
        return Ok(messages);
    }

    [HttpPost("/chats/summary")]
    public async Task<ActionResult<List<AiAnalysisResultDto>>> SummarizeChat(
        [Required] [FromQuery] long chatId,
        [Required] [FromQuery] [MinLength(1)] List<string> intends,
        [FromQuery] int count = 100,
        [FromQuery] string? verificationCode = null
    )
    {
        List<string> messages = await telegramService.GetMessagesAsync(chatId, count, verificationCode);
        List<AiAnalysisResultDto> analysisResult = await openAiService.AnalyzeAsync(messages, intends);
        return Ok(analysisResult);
    }
}
