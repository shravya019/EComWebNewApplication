using EComWebNewApplication.Data;
using EComWebNewApplication.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace EComWebNewApplication.Services
{
    public class ProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await _context.Products.ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _context.Products.FindAsync(id);
        }

        public async Task<List<Product>> GetExpensiveProductsAsync(decimal minPrice)
        {
            return await _context.Products
                                 .Where(p => p.Price > minPrice)
                                 .ToListAsync();
        }

        public async Task AddProductAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Product>> GetProductsByCategoryAsync(string category)
        {
            return await _context.Products
                                 .Where(p => p.Category == category)
                                 .ToListAsync();
        }
    }
}
