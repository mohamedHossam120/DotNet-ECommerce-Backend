using E_Commerce.DTOs.Carts;

namespace E_Commerce.Services.Carts;

public interface ICartService
{
    Task<CartResponseDto> CreateCartAsync(long userId);

    Task<CartResponseDto> GetCartAsync(
        long userId,
        long cartId);

    Task<CartResponseDto> AddToCartAsync(
        long userId,
        long cartId,
        AddToCartDto dto);

    Task<CartResponseDto> UpdateQuantityAsync(
        long userId,
        long cartId,
        long productId,
        int quantity);

    Task RemoveFromCartAsync(
        long userId,
        long cartId,
        long productId);

    Task ClearCartAsync(long userId, long cartId);

    Task DeleteCartAsync(long userId, long cartId);
}