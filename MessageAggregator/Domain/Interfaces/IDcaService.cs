using MessageAggregator.Domain.DTOs;

namespace MessageAggregator.Domain.Interfaces;

public interface IDcaService
{
    Task<AiAnalysisResultDto> AnalyzeAndSummarizeAsync(List<string> data);
}