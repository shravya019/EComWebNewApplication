using System.ComponentModel.DataAnnotations;

namespace EComWebNewApplication.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
    }
}
