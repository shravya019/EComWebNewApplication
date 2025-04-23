using EComWebNewApplication.Orders;

namespace EComWebNewApplication.Users
{
    public class Customer
    {
        public int Id { get; set; }
        public string? CustomerName { get; set; }
        public string? Email { get; set; }
        
        public string? Password { get; set; }
        public string? Phone { get; set; }
        public string? BillingAddress { get; set; }
        public List<Order>? Orders { get; set; }
    }
}
