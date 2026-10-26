using E_Commerce.DTOs.Products;

namespace E_Commerce.Services.Products;

public interface IProductService
{
    Task<ProductResponseDto> CreateAsync(CreateProductDto dto);
    Task<List<ProductResponseDto>> GetAllAsync();
    Task<ProductPagedResponseDto> GetPagedAsync(ProductQueryDto query);
    Task<ProductResponseDto> GetByIdAsync(long id);
    Task<ProductResponseDto> UpdateAsync(long id, UpdateProductDto dto);
    Task DeleteAsync(long id);
}