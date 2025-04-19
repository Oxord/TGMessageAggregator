using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs
{
    public class CreateCategoryDto
    {
        [Required]
        [StringLength(100, MinimumLength = 1)] // Example validation
        public string Name { get; set; } = string.Empty;
    }
}
