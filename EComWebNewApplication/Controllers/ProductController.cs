using EComWebNewApplication.Models;
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

        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllProductsAsync();
            return View(products);
        }
    }
}
