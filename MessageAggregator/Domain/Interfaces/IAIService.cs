using MessageAggregator.Domain.DTOs;

namespace MessageAggregator.Domain.Interfaces
{
    public interface IAIService
    {
        Task<CategorySummaryDto> AnalyzeAsync(string data);
    }
}
