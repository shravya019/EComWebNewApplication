using System.Net;

namespace EComWebNewApplication.Orders
{
    public class Order
    {
        public int OrderId { get; set; }
        public required string CustomerName { get; set; }
        public DateTime OrderDate { get; set; }

        public OrderStatus Status { get; set; }  
        public Address ShippingAddress { get; set; }
    }

}
