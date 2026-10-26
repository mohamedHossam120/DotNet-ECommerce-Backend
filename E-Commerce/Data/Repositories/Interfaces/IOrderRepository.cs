using E_Commerce.Models;

namespace E_Commerce.Data.Repositories.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(long id);

    Task<List<Order>> GetByUserIdAsync(long userId);

    Task AddOrderAsync(Order order);

    void Update(Order order);

    Task SaveChangesAsync();
}