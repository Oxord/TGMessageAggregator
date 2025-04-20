using System.Collections.Generic; // Added for List and IEnumerable
using System.Threading.Tasks;
using MessageAggregator.Domain.Models; // Added for Summary

namespace MessageAggregator.Domain.Interfaces;

public interface IDcaService
{
    // Added methods for retrieving summaries
    Task<IEnumerable<Summary>> GetAllSummariesAsync();
    Task<IEnumerable<Summary>> GetSummariesByCategoryAsync(string categoryName);
}
