using EComWebNewApplication.Products;
using EComWebNewApplication.Services;
using Microsoft.AspNetCore.Mvc;

namespace EComWebNewApplication.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductService _productService;

        public ProductController(ProductService productService)
        {
            _productService = productService;
        }

        public IActionResult Index()
        {
            var products = _productService.GetAllProducts();
            return View(products);
        }
    }

}
