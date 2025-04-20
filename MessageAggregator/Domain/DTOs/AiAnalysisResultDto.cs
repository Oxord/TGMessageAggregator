namespace MessageAggregator.Domain.DTOs;

public class AiAnalysisResultDto
{
    public string Summary { get; set; } = string.Empty;
    public string Intend { get; set; } = string.Empty;
}

public class AiSummaries
{
    public List<AiAnalysisResultDto> Results { get; set; } = [];
}