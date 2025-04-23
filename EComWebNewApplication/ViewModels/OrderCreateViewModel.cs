using System.ComponentModel.DataAnnotations;

namespace EComWebNewApplication.ViewModels
{
    public class OrderCreateViewModel
    {
        // In a real app, you would have a shopping cart.
        [Required]
        public List<OrderItemCreateViewModel> OrderItems { get; set; }
    }
    public class OrderItemCreateViewModel
    {
        [Required]
        public int ProductId { get; set; }
        [Required]
        public int Quantity { get; set; }
    }
}
