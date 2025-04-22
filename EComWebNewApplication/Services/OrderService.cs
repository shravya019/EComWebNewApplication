using EComWebNewApplication.Orders;
using EComWebNewApplication.Repositories;

namespace EComWebNewApplication.Services
{
    public class OrderService
    {
        private static readonly Repository<Order> _orderRepository=new Repository<Order>();
        private readonly ILogger<OrderService> _logger;
        private static bool _isInitialized = false;

        public OrderService(ILogger<OrderService> logger)
        {

           
            _logger = logger;


            if (!_isInitialized)
            {
                _orderRepository.Add(new Order { OrderId = 1, CustomerName = "Alice", OrderDate = new DateTime(2024, 5, 10) });
                _orderRepository.Add(new Order { OrderId = 2, CustomerName = "Bob", OrderDate = new DateTime(2024, 4, 5) });
                _orderRepository.Add(new Order { OrderId = 3, CustomerName = "Charlie", OrderDate = new DateTime(2024, 6, 1) });

                _isInitialized = true; // prevents adding again
            }
        }

        public List<Order> GetOrdersSortedByDate()
        {
            _logger.LogInformation("Getting orders sorted by date.");
            return _orderRepository.GetAll().OrderBy(o => o.OrderDate).ToList(); 
        }

        public async Task PlaceOrderAsync(Order order)
        {
            order.OrderId = new Random().Next(1000, 9999); // Or your own ID generator

            // Simulating order processing delay
            _logger.LogInformation($"Processing order for {order.CustomerName}...");

            await Task.Delay(2000); // Simulates a delay (e.g., processing time)

            // After the delay, add the order to the repository
            _orderRepository.Add(order);

            _logger.LogInformation($"Order for {order.CustomerName} has been processed.");
        }

        public async Task<string> GetOrderStatusAsync(int orderId)
        {
            // Simulate fetching order status from a background task
            return await Task.Run(() =>
            {
                // Simulating some processing time
                Task.Delay(1000).Wait(); // You can simulate a delay here too if needed

                // Simulate fetching the order's status (you could replace this with actual logic)
                Order order = _orderRepository.GetAll().FirstOrDefault(o => o.OrderId == orderId);

                return order != null ? $"Order {order.OrderId} status: Processed" : "Order not found";
            });
        }


    }

}
