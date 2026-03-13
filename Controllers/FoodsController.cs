using HCL_Food_Delivery.Contracts;
using HCL_Food_Delivery.Data;
using HCL_Food_Delivery.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HCL_Food_Delivery.Controllers;

[ApiController]
[Route("api/foods")]
public class FoodsController(FoodDeliveryDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FoodResponse>>> GetFoods([FromQuery] int? categoryId)
    {
        var query = dbContext.FoodItems
            .AsNoTracking()
            .Include(x => x.Category)
            .AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == categoryId);
        }

        var foods = await query
            .OrderBy(x => x.Name)
            .Select(x => new FoodResponse(
                x.Id,
                x.Name,
                x.Description,
                x.Price,
                x.CategoryId,
                x.Category != null ? x.Category.Name : null,
                x.ImageUrl,
                x.IsAvailable))
            .ToListAsync();

        return Ok(foods);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FoodResponse>> GetFoodById(int id)
    {
        var food = await dbContext.FoodItems
            .AsNoTracking()
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (food is null)
        {
            return NotFound();
        }

        return Ok(new FoodResponse(
            food.Id,
            food.Name,
            food.Description,
            food.Price,
            food.CategoryId,
            food.Category != null ? food.Category.Name : null,
            food.ImageUrl,
            food.IsAvailable));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<FoodResponse>> UpdateFood(int id, [FromBody] UpdateFoodRequest request)
    {
        var food = await dbContext.FoodItems
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (food is null)
        {
            return NotFound();
        }

        if (request.CategoryId.HasValue)
        {
            var categoryExists = await dbContext.Categories.AnyAsync(x => x.Id == request.CategoryId.Value);
            if (!categoryExists)
            {
                return BadRequest("Invalid categoryId.");
            }
        }

        food.Name = request.Name.Trim();
        food.Description = request.Description?.Trim();
        food.Price = request.Price;
        food.CategoryId = request.CategoryId;
        food.ImageUrl = request.ImageUrl?.Trim();
        food.IsAvailable = request.IsAvailable;

        dbContext.SaveChanges();

        return Ok(new FoodResponse(
            food.Id,
            food.Name,
            food.Description,
            food.Price,
            food.CategoryId,
            food.Category != null ? food.Category.Name : null,
            food.ImageUrl,
            food.IsAvailable));
    }
}
