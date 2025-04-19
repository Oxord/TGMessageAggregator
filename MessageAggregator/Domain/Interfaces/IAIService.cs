using MessageAggregator.Domain.DTOs;

namespace MessageAggregator.Domain.Interfaces;

public interface IAiService
{
    // Changed parameter type from string to IEnumerable<string>
    Task<AiAnalysisResultDto> AnalyzeAsync(IEnumerable<string> data);
}