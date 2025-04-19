using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MessageAggregator.Domain.Interfaces;
using MessageAggregator.Domain.DTOs;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DcaController : ControllerBase
    {
        private readonly IDcaService _dcaService;

        public DcaController(IDcaService dcaService)
        {
            _dcaService = dcaService;
        }

        // POST: api/dca/analyze
        [HttpPost("analyze")]
        public async Task<ActionResult<CategorySummaryDto>> Analyze([FromBody] AnalyzeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Data))
                return BadRequest("Data is required.");

            var result = await _dcaService.AnalyzeAndSummarizeAsync(request.Data);
            return Ok(result);
        }
    }

    public class AnalyzeRequest
    {
        public string Data { get; set; }
    }
}
