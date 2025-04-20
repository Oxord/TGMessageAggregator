using System.Threading.Tasks;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MessageAggregator.Domain.Interfaces;
using System;
using System.Collections.Generic;
using MessageAggregator.Domain.Models; // Added for List<string> and IEnumerable<Summary>

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DcaController : ControllerBase
    {
        private readonly IDcaService _dcaService;
        // Removed ICategoryRepository field

        public DcaController(IDcaService dcaService) // Removed repository from constructor
        {
            _dcaService = dcaService;
            // Removed repository assignment
        }

        // POST: api/dca/analyze
        [HttpPost("analyze")]
        public async Task<ActionResult<Summary>> Analyze([FromBody] AnalyzeRequest request) // Change return type to Summary
        {
            // Add null check for request itself before accessing Data
            if (request == null || request.Data == null || request.Data.Count < 1)
            {
                return BadRequest("Request body is invalid or data is required.");
            }
            // Add ChatName check
            if (string.IsNullOrWhiteSpace(request.ChatName))
            {
                return BadRequest("ChatName is required.");
            }

            // Call the service, passing ChatName
            var savedSummary = await _dcaService.AnalyzeAndSummarizeAsync(request.Data, request.ChatName); // Pass ChatName

            // Return the saved Summary object directly
            return Ok(savedSummary);
        }

        // GET: api/dca/summaries
        [HttpGet("summaries")]
        public async Task<ActionResult<IEnumerable<Summary>>> GetAllSummaries()
        {
            var summaries = await _dcaService.GetAllSummariesAsync();
            return Ok(summaries);
        }

        // GET: api/dca/summaries/{categoryName}
        [HttpGet("summaries/{categoryName}")]
        public async Task<ActionResult<IEnumerable<Summary>>> GetSummariesByCategory(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                return BadRequest("Category name cannot be empty.");
            }
            var summaries = await _dcaService.GetSummariesByCategoryAsync(categoryName);
            return Ok(summaries);
        }
    }

    public class AnalyzeRequest
    {
        public List<string> Data { get; set; } = new List<string>(); // Initialize
        public string? ChatName { get; set; } // Added ChatName property (nullable for safety)
    }
}
