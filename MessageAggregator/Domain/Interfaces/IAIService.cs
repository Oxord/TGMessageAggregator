using System.Collections.Generic;
using System.Threading.Tasks;
using MessageAggregator.Domain.Models;

namespace MessageAggregator.Domain.Interfaces;

public interface IAIService
{
    // Changed parameter type from string to IEnumerable<string>
    // Changed return type to Summary
    // Added chatName parameter
    Task<Summary> AnalyzeAsync(IEnumerable<string> data, string chatName);
}
