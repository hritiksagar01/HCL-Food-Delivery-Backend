using System.Security.Claims;
using System.Text.Json;
using HCL_Food_Delivery.Contracts;
using HCL_Food_Delivery.Data;
using HCL_Food_Delivery.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HCL_Food_Delivery.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController(FoodDeliveryDbContext dbContext) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<OrderSummaryResponse>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        if (!CanAccessUser(request.UserId))
        {
            return Forbid();
        }

        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == request.UserId);
        if (user is null)
        {
            return NotFound("User not found.");
        }

        var foodIds = request.Items.Select(x => x.FoodItemId).Distinct().ToList();
        var foods = await dbContext.FoodItems
            .Where(x => foodIds.Contains(x.Id) && x.IsAvailable)
            .ToDictionaryAsync(x => x.Id);

        if (foods.Count != foodIds.Count)
        {
            return BadRequest("One or more food items are unavailable.");
        }

        var orderItems = request.Items.Select(x => new OrderItem
        {
            FoodItemId = x.FoodItemId,
            Quantity = x.Quantity,
            Price = foods[x.FoodItemId].Price
        }).ToList();

        var calculatedTotal = orderItems.Sum(x => x.Price * x.Quantity);

        var order = new Order
        {
            UserId = request.UserId,
            TotalAmount = calculatedTotal,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Items = orderItems
        };

        dbContext.Orders.Add(order);
        dbContext.SaveChanges();

        var response = new OrderSummaryResponse(order.Id, order.UserId, order.TotalAmount, order.Status.ToString(), order.CreatedAt);
        return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, response);
    }

    [HttpGet("user/{userId:int}")]
    public async Task<ActionResult<IReadOnlyList<OrderSummaryResponse>>> GetOrdersByUser(int userId)
    {
        if (!CanAccessUser(userId))
        {
            return Forbid();
        }

        var orders = await dbContext.Orders
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new OrderSummaryResponse(
                x.Id,
                x.UserId,
                x.TotalAmount,
                x.Status.ToString(),
                x.CreatedAt))
            .ToListAsync();

        return Ok(orders);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FullOrderInfoResponse>> GetOrderById(int id)
    {
        var order = await dbContext.Orders
            .AsNoTracking()
            .Include(x => x.Items)
                .ThenInclude(x => x.FoodItem)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (order is null)
        {
            return NotFound();
        }

        var details = new FullOrderInfoResponse(
            new OrderSummaryResponse(order.Id, order.UserId, order.TotalAmount, order.Status.ToString(), order.CreatedAt),
            order.Items.Select(x => new OrderItemDetailResponse(
                x.Id,
                x.FoodItemId,
                x.FoodItem != null ? x.FoodItem.Name : string.Empty,
                x.Quantity,
                x.Price)).ToList());

        return Ok(details);
    }

    [HttpGet]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<IReadOnlyList<OrderSummaryResponse>>> GetAllOrders()
    {
        var orders = await dbContext.Orders
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new OrderSummaryResponse(
                x.Id,
                x.UserId,
                x.TotalAmount,
                x.Status.ToString(),
                x.CreatedAt))
            .ToListAsync();

        return Ok(orders);
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<OrderSummaryResponse>> UpdateStatus(int id, [FromBody] JsonElement payload)
    {
        var statusValue = payload.ValueKind switch
        {
            JsonValueKind.String => payload.GetString(),
            JsonValueKind.Object when payload.TryGetProperty("status", out var prop) => prop.GetString(),
            JsonValueKind.Object when payload.TryGetProperty("Status", out var prop) => prop.GetString(),
            _ => null
        };

        if (string.IsNullOrWhiteSpace(statusValue) || !Enum.TryParse<OrderStatus>(statusValue, true, out var parsedStatus))
        {
            return BadRequest("Invalid status.");
        }

        var order = await dbContext.Orders.FirstOrDefaultAsync(x => x.Id == id);
        if (order is null)
        {
            return NotFound();
        }

        order.Status = parsedStatus;
        dbContext.SaveChanges();

        return Ok(new OrderSummaryResponse(order.Id, order.UserId, order.TotalAmount, order.Status.ToString(), order.CreatedAt));
    }

    private bool CanAccessUser(int userId)
    {
        if (User.IsInRole(nameof(UserRole.Admin)))
        {
            return true;
        }

        var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claimValue, out var currentUserId) && currentUserId == userId;
    }
}
