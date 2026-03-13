using System.ComponentModel.DataAnnotations;

namespace HCL_Food_Delivery.Contracts;

public class CreateOrderRequest
{
    [Range(1, int.MaxValue)]
    public int UserId { get; set; }

    [Required]
    [MinLength(1)]
    public List<CreateOrderItemRequest> Items { get; set; } = [];

    [Range(0.01, 999999)]
    public decimal TotalAmount { get; set; }
}

public class CreateOrderItemRequest
{
    [Range(1, int.MaxValue)]
    public int FoodItemId { get; set; }

    [Range(1, 1000)]
    public int Quantity { get; set; }

    [Range(0.01, 999999)]
    public decimal Price { get; set; }
}

public record OrderSummaryResponse(
    int Id,
    int UserId,
    decimal TotalAmount,
    string Status,
    DateTime CreatedAt);

public record OrderItemDetailResponse(
    int Id,
    int FoodItemId,
    string FoodName,
    int Quantity,
    decimal Price);

public record FullOrderInfoResponse(
    OrderSummaryResponse Order,
    IReadOnlyList<OrderItemDetailResponse> Items);

public class UpdateOrderStatusRequest
{
    [Required]
    public string Status { get; set; } = string.Empty;
}
