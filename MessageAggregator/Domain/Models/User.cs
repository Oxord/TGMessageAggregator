using Microsoft.AspNetCore.Identity;

namespace MessageAggregator.Domain.Models
{
    public class User : IdentityUser
    {
        public string? TelegramSessionString { get; set; }
    }
}
