using EComWebNewApplication.Models;
using EComWebNewApplication.Orders;
using EComWebNewApplication.Products;
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

        // For Order entity
        modelBuilder.Entity<Order>()
            .OwnsOne(o => o.ShippingAddress);

        // For Product entity
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);
    }

    public DbSet<Order> Orders { get; set; }
    public DbSet<Product> Products { get; set; }
}
