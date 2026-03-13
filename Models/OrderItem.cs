using System.ComponentModel.DataAnnotations;

namespace HCL_Food_Delivery.Models;

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public int FoodItemId { get; set; }
    public FoodItem? FoodItem { get; set; }

    [Range(1, 1000)]
    public int Quantity { get; set; } = 1;

    public decimal Price { get; set; }
}
