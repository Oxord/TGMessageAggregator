using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MessageAggregator.Domain.Interfaces;
using MessageAggregator.Domain.DTOs;
// Add this for Summary and Category
using System;
using MessageAggregator.Domain.Models; // Add this for DateTime

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DcaController(IDcaService dcaService) : ControllerBase
    {
        // POST: api/dca/analyze
        [HttpPost("analyze")]
        public async Task<ActionResult<Summary>> Analyze([FromBody] AnalyzeRequest request) // Change return type to Summary
        {
            if (request?.Data.Count < 1)
            {
                return BadRequest("Data is required.");
            }

            // Get the analysis result (summary text, potential category ID, original category name)
            AiAnalysisResultDto analysisResult = await dcaService.AnalyzeAndSummarizeAsync(request.Data);

            // Create the Summary object (ChatName is omitted as requested)
            var summary = new Summary(
                chatName: "", // ChatName omitted
                description: analysisResult.SummaryText,
                createdAt: DateTime.UtcNow // Use current UTC time
            );

            return Ok(summary); // Return the created Summary object
        }
    }

    public class AnalyzeRequest
    {
        public List<string> Data { get; set; }
    }
}
