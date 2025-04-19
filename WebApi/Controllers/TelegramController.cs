using System.ComponentModel.DataAnnotations;
using MessageAggregator.Application.Service;
using Microsoft.AspNetCore.Mvc;
using TL;

namespace WebApi.Controllers;

[ApiController]
[Route("/api/telegram")]
public class TelegramController(TelegramService telegramService) : ControllerBase
{
    [HttpGet("/chats")]
    public async Task<ActionResult<List<ChatBase>>> GetAllChats(
        [Required][FromQuery] long chatId, 
        [FromQuery] int count = 100
    )
    {
        List<string> messages = await telegramService.GetMessagesAsync(chatId, count);
        return Ok(messages);
    }
}