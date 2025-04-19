namespace MessageAggregator.Domain.DTOs
{
    public class AiAnalysisResultDto
    {
        public string SummaryText { get; set; }
        public int? CategoryId { get; set; } // Nullable if category not found by name
        public string OriginalCategoryName { get; set; } // The category name suggested by AI
    }
}
