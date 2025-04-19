using System;

namespace Domain.Models
{
    public class Summary
    {
        public Guid Id { get; set; }
        public string ChatName { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public DateTime CreatedAt { get; set; }

        public Summary() { }

        public Summary(string chatName, string description, int categoryId, DateTime createdAt)
        {
            Id = Guid.NewGuid();
            ChatName = chatName;
            Description = description;
            CategoryId = categoryId;
            CreatedAt = createdAt;
        }
    }
}
