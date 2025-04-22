
using EComWebNewApplication.Products;
using EComWebNewApplication.Repositories;
namespace EComWebNewApplication.Services

{
    public class ProductService
    {
        private readonly Repository<IProduct> _repository;

        public ProductService()
        {
            _repository = new Repository<IProduct>();

            // Add initial products
            _repository.Add(new Product { ProductId = 1, Name = "Laptop", Price = 1200.00M ,Category="Electronics"});
            _repository.Add(new Product { ProductId = 2, Name = "Mouse", Price = 25.99M, Category = "Electronics" });
            _repository.Add(new Product { ProductId = 3, Name = "Monitor", Price = 300.00M, Category = "Electronics" });
        }

        public List<IProduct> GetAllProducts()
        {
            return _repository.GetAll().ToList();
        }

        public List<IProduct> GetExpensiveProducts(decimal minPrice)
        {
            return _repository.Find(p => p.Price > minPrice).ToList();
        }

        public void AddProduct(IProduct product)
        {
            _repository.Add(product);
        }

        public IProduct GetProductById(int id)
        {
            return _repository.Find(p => p.ProductId == id).FirstOrDefault();
        }

        public List<IProduct> GetProductsByCategory(string category)
        {
            return _repository.Find(p => p.Category == category).ToList();  
        }
    }

}
