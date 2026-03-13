using System.ComponentModel.DataAnnotations;

namespace HCL_Food_Delivery.Models;

public class FoodItem
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Range(0.01, 999999)]
    public decimal Price { get; set; }

    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    [MaxLength(255)]
    public string? ImageUrl { get; set; }

    public bool IsAvailable { get; set; } = true;
}
