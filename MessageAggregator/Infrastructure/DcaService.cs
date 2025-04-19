using System.Threading.Tasks;
using MessageAggregator.Domain.Interfaces;
using MessageAggregator.Domain.DTOs;

namespace Infrastructure
{
    public class DcaService : IDcaService
    {
        private readonly IAIService _aiService;

        public DcaService(IAIService aiService)
        {
            _aiService = aiService;
        }

        public async Task<CategorySummaryDto> AnalyzeAndSummarizeAsync(string data)
        {
            // Call AI service to get summary and category
            return await _aiService.AnalyzeAsync(data);
        }
    }
}
