using MessageAggregator.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.DTOs;
using Domain.Models; // Required for mapping
using System; // Required for ArgumentException

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: api/categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            var categoryDtos = categories.Select(c => new CategoryDto { Id = c.Id, Name = c.Name });
            return Ok(categoryDtos);
        }

        // GET: api/categories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetCategory(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);

            if (category == null)
            {
                return NotFound($"Category with ID {id} not found.");
            }

            var categoryDto = new CategoryDto { Id = category.Id, Name = category.Name };
            return Ok(categoryDto);
        }

        // POST: api/categories
        [HttpPost]
        public async Task<ActionResult<CategoryDto>> PostCategory([FromBody] CreateCategoryDto createCategoryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var newCategory = await _categoryService.CreateCategoryAsync(createCategoryDto.Name);
                var categoryDto = new CategoryDto { Id = newCategory.Id, Name = newCategory.Name };

                // Return 201 Created with the location of the new resource and the resource itself
                return CreatedAtAction(nameof(GetCategory), new { id = categoryDto.Id }, categoryDto);
            }
            catch (ArgumentException ex) // Catch specific exceptions from the service
            {
                // Return a BadRequest if the input was invalid (e.g., empty name)
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex) // Catch potential duplicate name exception
            {
                // Return a Conflict status code if the category already exists
                return Conflict(ex.Message);
            }
            catch (Exception ex) // Catch-all for other unexpected errors
            {
                // Log the exception ex
                return StatusCode(500, "An unexpected error occurred while creating the category.");
            }
        }

        // DELETE: api/categories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var deleted = await _categoryService.DeleteCategoryAsync(id);

            if (!deleted)
            {
                return NotFound($"Category with ID {id} not found.");
            }

            return NoContent(); // Standard response for successful deletion
        }

        // Helper method for mapping (could be moved to a dedicated mapper class/library like AutoMapper)
        // private CategoryDto MapToDto(Category category)
        // {
        //     return new CategoryDto { Id = category.Id, Name = category.Name };
        // }
    }
}
