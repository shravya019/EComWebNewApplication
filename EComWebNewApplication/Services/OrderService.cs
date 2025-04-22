using EComWebNewApplication.Data;
using EComWebNewApplication.Orders;
using Microsoft.EntityFrameworkCore;

public class OrderService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<OrderService> _logger;

    public OrderService(ApplicationDbContext context, ILogger<OrderService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Order>> GetOrdersSortedByDateAsync()
    {
        _logger.LogInformation("Getting orders sorted by date.");
        return await _context.Orders.OrderBy(o => o.OrderDate).ToListAsync();
    }

    public async Task PlaceOrderAsync(Order order)
    {
        order.OrderId = new Random().Next(1000, 9999); // optional if you use auto-generated IDs

        _logger.LogInformation($"Processing order for {order.CustomerName}...");

        await Task.Delay(2000); // simulate delay
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Order for {order.CustomerName} has been processed.");
    }

    public async Task<string> GetOrderStatusAsync(int orderId)
    {
        var order = await _context.Orders.FindAsync(orderId);

        return order != null ? $"Order {order.OrderId} status: Processed" : "Order not found";
    }
}
