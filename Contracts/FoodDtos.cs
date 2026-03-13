using System.ComponentModel.DataAnnotations;

namespace HCL_Food_Delivery.Contracts;

public record FoodResponse(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    int? CategoryId,
    string? CategoryName,
    string? ImageUrl,
    bool IsAvailable);

public class UpdateFoodRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Range(0.01, 999999)]
    public decimal Price { get; set; }

    public int? CategoryId { get; set; }

    [MaxLength(255)]
    public string? ImageUrl { get; set; }

    public bool IsAvailable { get; set; }
}

