using MessageAggregator.Domain.DTOs;

namespace MessageAggregator.Domain.Interfaces;

public interface IAiService
{
    // Changed parameter type from string to IEnumerable<string>
    Task<AiSummaries> AnalyzeAsync(List<ChatMessageDto> data, List<string> intends);
}
