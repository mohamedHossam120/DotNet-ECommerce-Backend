using E_Commerce.Models;

namespace E_Commerce.Data.Repositories.Interfaces;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(long id);

    Task<List<Category>> GetAllAsync();

    Task AddAsync(Category category);

    void Update(Category category);

    void Delete(Category category);

    Task<bool> ExistsAsync(long id);

    Task<bool> HasProductsAsync(long categoryId);

    Task<bool> HasChildrenAsync(long categoryId);

    Task SaveChangesAsync();
}