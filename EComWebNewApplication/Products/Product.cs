namespace EComWebNewApplication.Products
{
    public class Product : IProduct
    {
        public int ProductId { get; set; }
        public required string Name { get; set; }
        public decimal Price { get; set; }
        public required string Category { get; set; }

        public void DisplayProductInfo()
        {
            Console.WriteLine($"ID: {ProductId}, Name: {Name}, Price: {Price}");
        }
    }
}
