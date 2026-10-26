using E_Commerce.Models;

namespace E_Commerce.Data.Repositories.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(long id);

    Task<List<Product>> GetAllAsync();

    Task<(List<Product> Products, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? search,
        long? categoryId);

    Task<bool> ExistsBySkuAsync(string sku);

    Task<bool> CategoryExistsAsync(long categoryId);

    Task AddAsync(Product product);

    void Update(Product product);

    void Delete(Product product);

    Task SaveChangesAsync();
}