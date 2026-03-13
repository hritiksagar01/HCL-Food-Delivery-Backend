using System.ComponentModel.DataAnnotations;

namespace HCL_Food_Delivery.Models;

public class Category
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public ICollection<FoodItem> FoodItems { get; set; } = new List<FoodItem>();
}

