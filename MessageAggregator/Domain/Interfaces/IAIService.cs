using System.Collections.Generic;
using Domain.Models; // Added for Summary

namespace MessageAggregator.Domain.Interfaces;

public interface IAiService
{
    public interface IAIService
    {
        // Changed parameter type from string to IEnumerable<string>
        // Changed return type to Summary
        // Added chatName parameter
        Task<Summary> AnalyzeAsync(IEnumerable<string> data, string chatName);
    }
}
