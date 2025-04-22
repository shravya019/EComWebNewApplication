namespace EComWebNewApplication.Products
{
    public interface IProduct
    {
        int ProductId { get; set; }
        string Name { get; set; }
        decimal Price { get; set; }
        string Category { get; set; }

        void DisplayProductInfo();

    }
}
