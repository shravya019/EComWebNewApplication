using EComWebNewApplication.Services;
using Microsoft.AspNetCore.Mvc;
using EComWebNewApplication.Orders;

namespace EComWebNewApplication.Controllers
{
    public class OrderController : Controller
    {
        private readonly OrderService _orderService;

        public OrderController(OrderService orderService)
        {
            _orderService = orderService;
        }

        // Shows the list of orders
        public IActionResult Index()
        {
            List<Order> orders = _orderService.GetOrdersSortedByDate();
            return View(orders);
        }

        // ✅ GET method to show the order form
        [HttpGet]
        public IActionResult PlaceOrderForm()
        {
            return View(); // This will load PlaceOrderForm.cshtml
        }

        // ✅ POST method to place an order
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(Order order)
        {
            await _orderService.PlaceOrderAsync(order);
            return RedirectToAction("Index"); // Redirect to order list
        }

        // ✅ Optional: Get status of an order
        public async Task<IActionResult> GetOrderStatusAsync(int orderId)
        {
            string status = await _orderService.GetOrderStatusAsync(orderId);
            ViewData["OrderStatus"] = status;
            return View();
        }
    }
}
