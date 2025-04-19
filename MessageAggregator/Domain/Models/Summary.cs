namespace MessageAggregator.Domain.Models;

public class Summary(string chatName, string description, DateTime createdAt)
{
    public class Summary
    {
        public Guid Id { get; set; }
        public string? ChatName { get; set; } // Made nullable
        public string Description { get; set; }
        public string CategoryName { get; set; } // Changed from CategoryId/Category
        public DateTime CreatedAt { get; set; }

        // Initialize non-nullable strings in default constructor
        public Summary()
        {
            Description = string.Empty;
            CategoryName = string.Empty;
        }

        // Updated constructor
        public Summary(string chatName, string description, string categoryName, DateTime createdAt)
        {
            Id = Guid.NewGuid();
            ChatName = chatName;
            Description = description;
            CategoryName = categoryName; // Changed from CategoryId
            CreatedAt = createdAt;
        }
    }
}
