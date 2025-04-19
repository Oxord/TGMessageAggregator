using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MessageAggregator.Domain.Interfaces;
using MessageAggregator.Domain.DTOs;
using Domain.Models; // Add this for Summary and Category
using System; // Add this for DateTime

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DcaController : ControllerBase
    {
        private readonly IDcaService _dcaService;
        private readonly ICategoryRepository _categoryRepository; // Inject Category Repository

        public DcaController(IDcaService dcaService, ICategoryRepository categoryRepository) // Add repository to constructor
        {
            _dcaService = dcaService;
            _categoryRepository = categoryRepository; // Assign repository
        }

        // POST: api/dca/analyze
        [HttpPost("analyze")]
        public async Task<ActionResult<Summary>> Analyze([FromBody] AnalyzeRequest request) // Change return type to Summary
        {
            if (request?.Data.Count < 1)
            {
                return BadRequest("Data is required.");
            }

            // Get the analysis result (summary text, potential category ID, original category name)
            var analysisResult = await _dcaService.AnalyzeAndSummarizeAsync(request.Data);

            int categoryId;

            if (analysisResult.CategoryId.HasValue)
            {
                // Category exists, use its ID
                categoryId = analysisResult.CategoryId.Value;
            }
            else
            {
                // Category does not exist, create it
                var newCategory = new Category { Name = analysisResult.OriginalCategoryName };
                await _categoryRepository.AddAsync(newCategory);
                // Important: Need to retrieve the newly created category to get its ID
                // This assumes AddAsync saves changes and assigns an ID immediately.
                // A more robust approach might involve modifying AddAsync to return the ID/entity
                // or querying again by name. We'll query by name for now.
                var createdCategory = await _categoryRepository.GetByNameAsync(newCategory.Name);
                if (createdCategory == null)
                {
                    // Handle error: category creation failed or couldn't be retrieved
                    return StatusCode(500, "Failed to create or retrieve the new category.");
                }
                categoryId = createdCategory.Id;
            }

            // Create the Summary object (ChatName is omitted as requested)
            var summary = new Summary(
                chatName: null, // ChatName omitted
                description: analysisResult.SummaryText,
                categoryId: categoryId,
                createdAt: DateTime.UtcNow // Use current UTC time
            );

            // TODO: Persist the summary object using a SummaryRepository/Service if needed

            return Ok(summary); // Return the created Summary object
        }
    }

    public class AnalyzeRequest
    {
        public List<string> Data { get; set; }
    }
}
