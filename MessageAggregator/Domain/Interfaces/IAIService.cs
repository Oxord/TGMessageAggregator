using System.Collections.Generic; // Added for IEnumerable
using MessageAggregator.Domain.DTOs;

namespace MessageAggregator.Domain.Interfaces
{
    public interface IAIService
    {
        // Changed parameter type from string to IEnumerable<string>
        Task<AiAnalysisResultDto> AnalyzeAsync(IEnumerable<string> data);
    }
}
