using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace EComWebNewApplication.Orders
{
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderId { get; set; }

        public string CustomerName { get; set; }
        public DateTime OrderDate { get; set; }

        public OrderStatus Status { get; set; }  
        public Address ShippingAddress { get; set; }
    }

}
