using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessageAggregator.Domain.Interfaces;
using MessageAggregator.Domain.Models;
using MessageAggregator.Infrastructure; // Added for AppDbContext
using Microsoft.EntityFrameworkCore; // Added for ToListAsync, Where

namespace Infrastructure
{
    public class DcaService : IDcaService
    {
        private readonly AppDbContext _dbContext; // Added DbContext

        // Inject AppDbContext
        public DcaService(AppDbContext dbContext)
        {
            _dbContext = dbContext; // Assign DbContext
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
