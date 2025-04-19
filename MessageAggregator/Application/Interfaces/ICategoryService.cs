using Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MessageAggregator.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync();

        Task<Category?> GetCategoryByIdAsync(int id);

        Task<Category> CreateCategoryAsync(string name); // Takes name, returns created category

        Task<bool> DeleteCategoryAsync(int id); // Returns true if deleted, false otherwise
    }
}
