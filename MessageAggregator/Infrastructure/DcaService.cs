using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessageAggregator.Domain.Interfaces;
using Domain.Models; // Added for Summary
using MessageAggregator.Infrastructure; // Added for AppDbContext
using Microsoft.EntityFrameworkCore; // Added for ToListAsync, Where

namespace Infrastructure
{
    public class DcaService : IDcaService
    {
        private readonly IAIService _aiService;
        private readonly AppDbContext _dbContext; // Added DbContext

        // Inject AppDbContext
        public DcaService(IAIService aiService, AppDbContext dbContext)
        {
            _aiService = aiService;
            _dbContext = dbContext; // Assign DbContext
        }

        // Updated return type to Summary and added chatName parameter
        public async Task<Summary> AnalyzeAndSummarizeAsync(List<string> data, string chatName) // Added chatName parameter
        {
            // Call AI service, passing chatName
            return await _aiService.AnalyzeAsync(data, chatName); // Pass chatName
        }

        // Implementation for getting all summaries
        public async Task<IEnumerable<Summary>> GetAllSummariesAsync()
        {
            return await _dbContext.Summaries.ToListAsync();
        }

        // Implementation for getting summaries by category
        public async Task<IEnumerable<Summary>> GetSummariesByCategoryAsync(string categoryName)
        {
            return await _dbContext.Summaries
                                   .Where(s => s.CategoryName == categoryName)
                                   .ToListAsync();
        }
    }
}
