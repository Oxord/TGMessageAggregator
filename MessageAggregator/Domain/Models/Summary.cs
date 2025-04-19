namespace MessageAggregator.Domain.Models;

public class Summary(string chatName, string description, DateTime createdAt)
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ChatName { get; set; } = chatName;
    public string Description { get; set; } = description;
    public DateTime CreatedAt { get; set; } = createdAt;
}