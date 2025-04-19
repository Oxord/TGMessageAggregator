using WTelegram;
using TL;
using Microsoft.Extensions.Options;

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

    // WTelegramClient требует обработчик конфигурации для авторизации

    // Обработчик конфигурации для WTelegramClient
    private string Config(string what)
    {
        return what switch
        {
            "api_id" => _settings.AppId,
            "api_hash" => _settings.ApiHash,
            "phone_number" => _settings.PhoneNumber,
            _ => null!,
        };
    }

    public async Task<List<string>> GetMessagesAsync(long chatIdentifier, int count)
    {
        await _client.LoginUserIfNeeded();

        // Поиск чата по username или ID
        var dialogs = await _client.Messages_GetAllDialogs();
        ChatBase? chat = dialogs.chats.Values.FirstOrDefault(c =>
            (c is Channel channelObj && channelObj.username == chatIdentifier.ToString()) ||
            c.ID == chatIdentifier
        );

        if (chat == null)
        {
            throw new Exception("Чат не найден");
        }

        // Получение сообщений
        Messages_MessagesBase? messagesBase = await _client.Messages_GetHistory(chat, limit: count);
        IEnumerable<Message> messageList = messagesBase switch
        {
            Messages_Messages mm => mm.messages.OfType<Message>(),
            Messages_ChannelMessages mcm => mcm.messages.OfType<Message>(),
            _ => [],
        };

        List<string> texts = messageList
            .Where(m => !string.IsNullOrEmpty(m.message))
            .OrderBy(m => m.date)
            .Select(m => m.message)
            .ToList();

        return texts;
    }

    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }
}