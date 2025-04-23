using EComWebNewApplication.Models;
using EComWebNewApplication.Orders;
using EComWebNewApplication.Products;
using EComWebNewApplication.Users;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EComWebNewApplication.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    //creating order table




    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Seed Customers (2 regular and 1 admin).
        modelBuilder.Entity<Customer>().HasData(
            new Customer { Id = 1, CustomerName = "Pranaya Rout", Email = "pranayakumar7@gmail.com", Password = "password", Phone = "1234567890", BillingAddress = "123 Main St" },
            new Customer { Id = 2, CustomerName = "Mitali Rout", Email = "pranayakumar777@gmail.com", Password = "password", Phone = "0987654321", BillingAddress = "456 Park Ave" },
            new Customer { Id = 3, CustomerName = "Admin", Email = "admin@example.com", Password = "admin", Phone = "1112223333", BillingAddress = "Admin HQ" }
        );
        // Seed Products.
        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = 1,
                ProductName = "Apple iPhone 13",
                Category = "Smartphones",
                Description = "The latest Apple iPhone featuring the A15 Bionic chip, 5G connectivity, and an advanced dual-camera system.",
                UnitPrice = 799.00m
            },
            new Product
            {
                Id = 2,
                ProductName = "Samsung Galaxy S21",
                Category = "Smartphones",
                Description = "A high-end smartphone with a 6.2-inch display, Exynos 2100 processor, and a versatile triple-camera setup.",
                UnitPrice = 699.00m
            },
            new Product
            {
                Id = 3,
                ProductName = "Sony WH-1000XM4 Headphones",
                Category = "Audio",
                Description = "Industry-leading noise-canceling headphones with superior sound quality and long battery life.",
                UnitPrice = 349.99m
            },
            new Product
            {
                Id = 4,
                ProductName = "Dell XPS 13 Laptop",
                Category = "Computers",
                Description = "A sleek and powerful ultrabook featuring a 13.4-inch display, Intel Core i7 processor, and fast SSD storage.",
                UnitPrice = 999.99m
            },
            new Product
            {
                Id = 5,
                ProductName = "Amazon Echo Dot (4th Gen)",
                Category = "Smart Home",
                Description = "A compact smart speaker with Alexa voice assistant for controlling smart home devices and streaming music.",
                UnitPrice = 49.99m
            }
        );
    }
    public DbSet<Template> Templates { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Payment> Payments { get; set; }
}

