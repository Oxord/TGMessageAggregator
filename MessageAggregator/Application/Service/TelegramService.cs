using WTelegram;
using TL;
using Microsoft.Extensions.Options;
using MessageAggregator.Domain.DTOs;

namespace MessageAggregator.Application.Service;

public class TelegramService : IDisposable
{
    private readonly TelegramSettings _settings;
    private readonly Client _client;

    public TelegramService(IOptions<TelegramSettings> options)
    {
        _settings = options.Value;
        _client = new Client(Config);
    }

    private string Config(string what)
    {
        return what switch
        {
            "api_id" => _settings.AppId,
            "api_hash" => _settings.ApiHash,
            "phone_number" => _settings.PhoneNumber,
            "verification_code" => _settings.VerificationCode,
            _ => null!,
        };
    }

    public async Task<List<ChatMessageDto>> GetMessagesAsync(
        long chatIdentifier,
        int count,
        string? verificationCode = null
    )
    {
        _settings.VerificationCode = verificationCode ?? string.Empty;
        await _client.LoginUserIfNeeded();

        Messages_Dialogs dialogs = await _client.Messages_GetAllDialogs();

        ChatBase chat = dialogs.chats.Values.First(c => c.ID == chatIdentifier);

        Messages_MessagesBase messagesBase = await _client.Messages_GetHistory(chat, count);
        // Получаем коллекцию пользователей из ответа Telegram API
        Dictionary<long, User> users = messagesBase switch
        {
            Messages_Messages mm => mm.users,
            Messages_ChannelMessages mcm => mcm.users,
            _ => new Dictionary<long, User>(),
        };

        IEnumerable<Message> messageList = messagesBase switch
        {
            Messages_Messages mm => mm.messages.OfType<Message>(),
            Messages_ChannelMessages mcm => mcm.messages.OfType<Message>(),
            _ => [],
        };

        List<ChatMessageDto> chatMessages = messageList
            .Where(m => !string.IsNullOrEmpty(m.message))
            .OrderBy(m => m.date)
            .Select(m => new ChatMessageDto
            {
                Message = m.message,
                Sender = users.TryGetValue(m.From.ID, out var user)
                    ? $"{user.first_name} {user.last_name}".Trim()
                        .Replace("  ", " ")
                        .Trim()
                    : m.From.ID.ToString(),
            })
            .ToList();

        return chatMessages;
    }

    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }
}
