using System.ComponentModel.DataAnnotations;
using MessageAggregator.Application.Service;
using MessageAggregator.Domain.DTOs;
using MessageAggregator.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using TL;

namespace WebApi.Controllers;

[ApiController]
[Route("/api/telegram")]
public class TelegramController(TelegramService telegramService, IDcaService dcaService) : ControllerBase
{
    [HttpGet("/chats")]
    public async Task<ActionResult<List<ChatBase>>> GetAllChats(
        [Required] [FromQuery] long chatId,
        [FromQuery] int count = 100
    )
    {
        List<string> messages = await telegramService.GetMessagesAsync(chatId, count);
        return Ok(messages);
    }

    [HttpPost("/chats/summary")]
    public async Task<ActionResult<AiAnalysisResultDto>> SummarizeChat(
        [Required] [FromQuery] long chatId,
        [FromQuery] int count = 100
    )
    {
        List<string> messages = await telegramService.GetMessagesAsync(chatId, count);
        AiAnalysisResultDto analysisResult = await dcaService.AnalyzeAndSummarizeAsync(messages);
        return Ok(analysisResult);
    }
}