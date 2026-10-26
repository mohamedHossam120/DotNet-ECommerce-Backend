using System.ComponentModel.DataAnnotations;
using E_Commerce.Models.Enums;

namespace E_Commerce.DTOs.Orders;

public class UpdateOrderStatusDto
{
    [EnumDataType(typeof(OrderStatus))]
    public OrderStatus Status { get; set; }
}