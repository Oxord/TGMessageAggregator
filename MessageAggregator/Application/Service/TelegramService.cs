using WTelegram;
using TL;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http; // Added for IHttpContextAccessor
using Microsoft.AspNetCore.Identity; // Added for UserManager
using MessageAggregator.Domain.Models; // Added for User
using System.Security.Claims; // Added for ClaimTypes

namespace MessageAggregator.Application.Service;

public class TelegramService : IDisposable
{
    private readonly TelegramSettings _settings;
    private readonly Client _client;
    private readonly string _sessionString; // Store the user-specific session

    // Inject HttpContextAccessor and UserManager
    public TelegramService(
        IOptions<TelegramSettings> options,
        IHttpContextAccessor httpContextAccessor,
        UserManager<MessageAggregator.Domain.Models.User> userManager) // Fully qualified User
    {
        _settings = options.Value;

        // --- Get User-Specific Session ---
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            throw new InvalidOperationException("Cannot initialize TelegramService outside of an HTTP request context.");
        }

        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            // This should ideally not happen if the controller endpoint is [Authorize]
            throw new InvalidOperationException("User is not authenticated.");
        }

        // FindAsync is async, but constructors cannot be async.
        // We need to block synchronously here, which is generally discouraged,
        // but necessary in a constructor if the dependency is required for initialization.
        // Consider alternative patterns (e.g., async factory) if this becomes problematic.
        var user = userManager.FindByIdAsync(userId).GetAwaiter().GetResult();
        if (user == null)
        {
             throw new InvalidOperationException($"User with ID {userId} not found.");
        }

        if (string.IsNullOrEmpty(user.TelegramSessionString))
        {
            throw new InvalidOperationException("User has not linked their Telegram account or session is invalid.");
        }
        _sessionString = user.TelegramSessionString;
        // --- End Get User-Specific Session ---


        // Initialize client with the user's session
        _client = new Client(ConfigProvider);
    }


    // Updated ConfigProvider to use the user's session string
    private string? ConfigProvider(string what) // Return string?
    {
        return what switch
        {
            "api_id" => _settings.AppId,
            "api_hash" => _settings.ApiHash,
            "session" => _sessionString, // Provide the loaded session string
            // No phone_number needed when using session string
            _ => null, // Return null for unhandled cases
        };
    }

    public async Task<List<string>> GetMessagesAsync(long chatIdentifier, int count)
    {
        // Remove LoginUserIfNeeded - client is initialized with session
        // await _client.LoginUserIfNeeded();

        // Check connection status (optional but recommended)
        // This might throw if the session is invalid/expired
        try
        {
             await _client.LoginUserIfNeeded(); // Or just check _client.User status if available
        }
        catch (Exception ex)
        {
             // Handle potential session errors (e.g., session revoked)
             throw new InvalidOperationException($"Telegram session error: {ex.Message}. Please re-link your account.", ex);
        }


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
