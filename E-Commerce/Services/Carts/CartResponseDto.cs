namespace E_Commerce.DTOs.Carts;

public class CartResponseDto
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public List<CartItemResponseDto> Items { get; set; } = new();
}

public class CartItemResponseDto
{
    public long ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal TotalPrice => UnitPrice * Quantity;
}