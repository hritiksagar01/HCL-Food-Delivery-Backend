using HCL_Food_Delivery.Contracts;
using HCL_Food_Delivery.Data;
using HCL_Food_Delivery.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HCL_Food_Delivery.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = nameof(UserRole.Admin))]
public class AdminController(FoodDeliveryDbContext dbContext) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<ActionResult<AdminDashboardResponse>> GetDashboard()
    {
        var totalUsers = await dbContext.Users.CountAsync(x => x.Role == UserRole.Customer);
        var totalFoodItems = await dbContext.FoodItems.CountAsync();
        var totalOrders = await dbContext.Orders.CountAsync();
        var pendingOrders = await dbContext.Orders.CountAsync(x => x.Status == OrderStatus.Pending || x.Status == OrderStatus.Preparing);
        var outForDelivery = await dbContext.Orders.CountAsync(x => x.Status == OrderStatus.OutForDelivery);
        var delivered = await dbContext.Orders.CountAsync(x => x.Status == OrderStatus.Delivered);

        var startOfTodayUtc = DateTime.UtcNow.Date;
        var ordersPlacedToday = await dbContext.Orders.CountAsync(x => x.CreatedAt >= startOfTodayUtc);

        var recentOrders = await dbContext.Orders
            .AsNoTracking()
            .Include(x => x.User)
            .OrderByDescending(x => x.CreatedAt)
            .Take(10)
            .Select(x => new AdminRecentOrderResponse(
                x.Id,
                x.User != null ? x.User.Name : string.Empty,
                x.Status,
                x.TotalAmount,
                x.CreatedAt))
            .ToListAsync();

        var dashboard = new AdminDashboardResponse(
            totalUsers,
            totalFoodItems,
            totalOrders,
            pendingOrders,
            outForDelivery,
            delivered,
            ordersPlacedToday,
            recentOrders);

        return Ok(dashboard);
    }
}
