using System.Threading.Tasks;
using MessageAggregator.Domain.Interfaces;
using MessageAggregator.Domain.DTOs;

namespace Infrastructure
{
    public class AiService : IAIService
    {
        public async Task<CategorySummaryDto> AnalyzeAsync(string data)
        {
            // TODO: Replace with real AI API call
            await Task.Delay(100); // Simulate async work

            // Dummy logic: if data contains "error", category is "Error", else "Info"
            var category = data.ToLower().Contains("error") ? "Error" : "Info";
            var summary = data.Length > 50 ? data.Substring(0, 50) + "..." : data;

            return new CategorySummaryDto
            {
                Summary = summary,
                Category = category
            };
        }
    }
}
