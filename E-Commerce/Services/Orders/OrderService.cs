using E_Commerce.Data;
using E_Commerce.DTOs.Orders;
using E_Commerce.Models;
using E_Commerce.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Services.Orders;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;

    public OrderService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<OrderResponseDto> CreateAsync(
        long userId,
        CreateOrderDto dto)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null || !cart.Items.Any())
        {
            throw new InvalidOperationException(
                "Your cart is empty.");
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        var order = new Order
        {
            UserId = userId,
            Status = OrderStatus.Pending,
            ShippingAddressLine1 = dto.ShippingAddressLine1,
            ShippingAddressLine2 = dto.ShippingAddressLine2,
            ShippingCity = dto.ShippingCity,
            ShippingCountry = dto.ShippingCountry,
            ShippingPostalCode = dto.ShippingPostalCode,
            ShippingPhoneNumber = dto.ShippingPhoneNumber,
            CreatedAtUtc = DateTime.UtcNow,
            Items = new List<OrderItem>()
        };

        foreach (var cartItem in cart.Items)
        {
            var product = cartItem.Product;

            if (product == null || product.IsDeleted)
            {
                throw new InvalidOperationException(
                    "A product in your cart is unavailable.");
            }

            if (product.Status != ProductStatus.Active)
            {
                throw new InvalidOperationException(
                    $"Product {product.Name} is not available.");
            }

            if (cartItem.Quantity <= 0 ||
                product.StockQuantity < cartItem.Quantity)
            {
                throw new InvalidOperationException(
                    $"Insufficient stock for {product.Name}.");
            }

            var orderItem = new OrderItem
            {
                ProductId = product.Id,
                Product = product,
                Quantity = cartItem.Quantity,
                UnitPrice = product.Price
            };

            order.Items.Add(orderItem);

            order.TotalAmount +=
                product.Price * cartItem.Quantity;

            product.StockQuantity -= cartItem.Quantity;

            if (product.StockQuantity == 0)
            {
                product.Status = ProductStatus.OutOfStock;
            }
        }

        _context.Orders.Add(order);

        _context.CartItems.RemoveRange(cart.Items);

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return MapToDto(order);
    }

    public async Task<OrderResponseDto> GetByIdAsync(
        long userId,
        long orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o =>
                o.Id == orderId &&
                o.UserId == userId &&
                !o.IsDeleted);

        if (order == null)
        {
            throw new KeyNotFoundException(
                "Order not found.");
        }

        return MapToDto(order);
    }

    public async Task<List<OrderResponseDto>> GetByUserIdAsync(
        long userId)
    {
        var orders = await _context.Orders
            .Where(o =>
                o.UserId == userId &&
                !o.IsDeleted)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync();

        return orders.Select(MapToDto).ToList();
    }


    public async Task<OrderResponseDto> UpdateStatusAsync(
        long orderId,
        OrderStatus status)
    {
        if (!Enum.IsDefined(typeof(OrderStatus), status))
        {
            throw new ArgumentException("Invalid order status.");
        }

        var order = await _context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o =>
                o.Id == orderId &&
                !o.IsDeleted);

        if (order == null)
        {
            throw new KeyNotFoundException("Order not found.");
        }

        if (order.Status == status)
        {
            return MapToDto(order);
        }

        var allowed = order.Status switch
        {
            OrderStatus.Pending =>
                status == OrderStatus.Processing ||
                status == OrderStatus.Cancelled,

            OrderStatus.Processing =>
                status == OrderStatus.Shipped ||
                status == OrderStatus.Cancelled,

            OrderStatus.Shipped =>
                status == OrderStatus.Delivered,

            OrderStatus.Delivered => false,
            OrderStatus.Cancelled => false,

            _ => false
        };

        if (!allowed)
        {
            throw new InvalidOperationException(
                $"Cannot change order status from {order.Status} to {status}.");
        }

        if (status == OrderStatus.Cancelled)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            foreach (var item in order.Items)
            {
                if (item.Product == null || item.IsDeleted)
                {
                    continue;
                }

                item.Product.StockQuantity += item.Quantity;

                if (item.Product.Status == ProductStatus.OutOfStock &&
                    item.Product.StockQuantity > 0)
                {
                    item.Product.Status = ProductStatus.Active;
                }
            }

            order.Status = OrderStatus.Cancelled;
            order.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        else
        {
            order.Status = status;
            order.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        return MapToDto(order);
    }


    public async Task CancelAsync(
        long userId,
        long orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o =>
                o.Id == orderId &&
                o.UserId == userId &&
                !o.IsDeleted);

        if (order == null)
        {
            throw new KeyNotFoundException(
                "Order not found.");
        }

        if (order.Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException(
                "Only pending orders can be cancelled.");
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        foreach (var item in order.Items)
        {
            if (item.Product == null)
            {
                continue;
            }

            item.Product.StockQuantity += item.Quantity;

            if (item.Product.Status == ProductStatus.OutOfStock &&
                item.Product.StockQuantity > 0)
            {
                item.Product.Status = ProductStatus.Active;
            }
        }

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    private static OrderResponseDto MapToDto(Order order)
    {
        return new OrderResponseDto
        {
            Id = order.Id,
            UserId = order.UserId,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            ShippingAddressLine1 = order.ShippingAddressLine1,
            ShippingAddressLine2 = order.ShippingAddressLine2,
            ShippingCity = order.ShippingCity,
            ShippingCountry = order.ShippingCountry,
            ShippingPostalCode = order.ShippingPostalCode,
            ShippingPhoneNumber = order.ShippingPhoneNumber,
            CreatedAtUtc = order.CreatedAtUtc,

            Items = order.Items
                .Where(i => !i.IsDeleted)
                .Select(i => new OrderItemResponseDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                })
                .ToList()
        };
    }
}