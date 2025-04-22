using EComWebNewApplication.Orders;
using System.ComponentModel.DataAnnotations;

namespace EComWebNewApplication.Models
{
    public class Template
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(200)]
        public required string TemplateName { get; set; }
        // HTML content with placeholders (e.g., {CustomerName}, {OrderDate}, {ProductList}, {TotalAmount})
        [Required]
        public required string HtmlContent { get; set; }
        // The type of invoice template.
        public OrderStatus orderStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public required string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
