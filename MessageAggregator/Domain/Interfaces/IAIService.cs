using MessageAggregator.Domain.DTOs;

namespace MessageAggregator.Domain.Interfaces;

public interface IAiService
{
    // Changed parameter type from string to IEnumerable<string>
    Task<List<AiAnalysisResultDto>> AnalyzeAsync(List<string> data, List<string> intends);
}
