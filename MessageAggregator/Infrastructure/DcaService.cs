using MessageAggregator.Domain.DTOs;
using MessageAggregator.Domain.Interfaces;

namespace MessageAggregator.Infrastructure;

public class DcaService(IAiService aiService) : IDcaService
{
    // Update return type to match interface
    public async Task<AiAnalysisResultDto> AnalyzeAndSummarizeAsync(List<string> data)
    {
        // Call AI service to get the analysis result DTO
        return await aiService.AnalyzeAsync(data);
    }
}