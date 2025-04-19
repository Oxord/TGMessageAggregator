using Domain.Models;
using MessageAggregator.Application.Interfaces;
using MessageAggregator.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MessageAggregator.Application.Services // Assuming MessageAggregator.Application.Services namespace
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _categoryRepository.GetAllAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _categoryRepository.GetByIdAsync(id);
        }

        public async Task<Category> CreateCategoryAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Category name cannot be empty.", nameof(name));
            }

            // Optional: Check if category with the same name already exists
            // var existingCategory = await _categoryRepository.GetByNameAsync(name); // Requires adding GetByNameAsync to repository
            // if (existingCategory != null)
            // {
            //     throw new InvalidOperationException($"Category with name '{name}' already exists.");
            // }

            var newCategory = new Category { Name = name };
            await _categoryRepository.AddAsync(newCategory);
            // The ID will be populated by the database after AddAsync completes (assuming identity column)
            return newCategory;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var exists = await _categoryRepository.ExistsAsync(id);
            if (!exists)
            {
                return false; // Or throw NotFoundException
            }

            await _categoryRepository.DeleteAsync(id);
            return true;
        }
    }
}
