namespace MessageAggregator.Domain.DTOs;

public class AiAnalysisResultDto
{
    public string Summary { get; set; }
    public string Intend { get; set; }
}

public class AiSummaries
{
    public List<AiAnalysisResultDto> Results { get; set; } = [];
}