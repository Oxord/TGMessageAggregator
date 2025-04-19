using System.Collections.Generic; // Added for List and IEnumerable
using System.Threading.Tasks;
using MessageAggregator.Domain.Models; // Added for Summary

namespace MessageAggregator.Domain.Interfaces;

public interface IDcaService
{
    // Changed return type from AiAnalysisResultDto to Summary
    // Added chatName parameter
    Task<Summary> AnalyzeAndSummarizeAsync(List<string> data, string chatName);

    // Added methods for retrieving summaries
    Task<IEnumerable<Summary>> GetAllSummariesAsync();
    Task<IEnumerable<Summary>> GetSummariesByCategoryAsync(string categoryName);
}
