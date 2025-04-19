namespace WebApi.DTOs
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Optional: Add other properties if needed, e.g., number of summaries
        // public int SummaryCount { get; set; }
    }
}
