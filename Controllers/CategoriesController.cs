using HCL_Food_Delivery.Contracts;
using HCL_Food_Delivery.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HCL_Food_Delivery.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController(FoodDeliveryDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> GetCategories()
    {
        var categories = await dbContext.Categories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new CategoryResponse(x.Id, x.Name))
            .ToListAsync();

        return Ok(categories);
    }
}
