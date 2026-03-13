using HCL_Food_Delivery.Models;

namespace HCL_Food_Delivery.Contracts;

public record AdminDashboardResponse(
    int TotalUsers,
    int TotalFoodItems,
    int TotalOrders,
    int PendingOrders,
    int OutForDeliveryOrders,
    int DeliveredOrders,
    int OrdersPlacedToday,
    IReadOnlyList<AdminRecentOrderResponse> RecentOrders);

public record AdminRecentOrderResponse(
    int OrderId,
    string UserName,
    OrderStatus Status,
    decimal TotalAmount,
    DateTime CreatedAt);
