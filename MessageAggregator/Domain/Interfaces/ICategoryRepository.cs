using Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MessageAggregator.Domain.Interfaces
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync();

        Task<Category?> GetByIdAsync(int id);

        Task<Category?> GetByNameAsync(string name);

        Task AddAsync(Category category);

        Task DeleteAsync(int id);

        Task<bool> ExistsAsync(int id); // Added for checking before deletion
    }
}
