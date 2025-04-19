using System.Threading.Tasks;
using MessageAggregator.Domain.DTOs;

namespace MessageAggregator.Domain.Interfaces
{
    public interface IDcaService
    {
        Task<CategorySummaryDto> AnalyzeAndSummarizeAsync(string data);
    }
}
