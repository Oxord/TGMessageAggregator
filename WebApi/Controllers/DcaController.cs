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
    public class DcaController(IDcaService dcaService, IAiService _aiService) : ControllerBase
    {
        // Removed ICategoryRepository field

        // Removed repository from constructor
        // Removed repository assignment

        // GET: api/dca/summaries
        [HttpGet("summaries")]
        public async Task<ActionResult<IEnumerable<Summary>>> GetAllSummaries()
        {
            var summaries = await dcaService.GetAllSummariesAsync();
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
            var summaries = await dcaService.GetSummariesByCategoryAsync(categoryName);
            return Ok(summaries);
        }
    }

    public class AnalyzeRequest
    {
        public List<string> Data { get; set; } = new List<string>(); // Initialize
        public string? ChatName { get; set; } // Added ChatName property (nullable for safety)
    }
}
