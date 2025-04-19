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

        // Update return type to match interface
        public async Task<AiAnalysisResultDto> AnalyzeAndSummarizeAsync(List<string> data)
        {
            // Call AI service to get the analysis result DTO
            return await _aiService.AnalyzeAsync(data);
        }
    }
}
