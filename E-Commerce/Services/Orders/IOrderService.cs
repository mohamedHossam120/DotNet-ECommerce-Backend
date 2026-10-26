using E_Commerce.DTOs.Orders;
using E_Commerce.Models.Enums;

namespace E_Commerce.Services.Orders;

public interface IOrderService
{
    Task<OrderResponseDto> CreateAsync(
        long userId,
        CreateOrderDto dto);

    Task<OrderResponseDto> GetByIdAsync(
        long userId,
        long orderId);

    Task<List<OrderResponseDto>> GetByUserIdAsync(
        long userId);

    Task<OrderResponseDto> UpdateStatusAsync(
        long orderId,
        OrderStatus status);

    Task CancelAsync(
        long userId,
        long orderId);
}