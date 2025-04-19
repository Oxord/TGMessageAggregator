namespace MessageAggregator.Domain.DTOs;

public class AiAnalysisResultDto
{
    public string SummaryText { get; set; }
    public string OriginalCategoryName { get; set; } // The category name suggested by AI
}