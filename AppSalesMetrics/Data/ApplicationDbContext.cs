using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AppSalesMetrics.Models;
using AppSalesMetrics.Shared.Models;

namespace AppSalesMetrics.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<SalesMetric> SalesMetric => Set<SalesMetric>();
    public DbSet<Customer> Customer => Set<Customer>();
    public DbSet<Category> Category => Set<Category>();
    public DbSet<OrderItem> OrderItem => Set<OrderItem>();
    public DbSet<Order> Order => Set<Order>();
    public DbSet<Product> Product => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SalesMetric>()
            .Property(e => e.TotalSales)
            .HasConversion<double>();
        modelBuilder.Entity<SalesMetric>()
            .Property(e => e.TotalSales)
            .HasPrecision(19, 4);
        modelBuilder.Entity<Category>()
            .HasMany(x => x.Product);
        modelBuilder.Entity<OrderItem>()
            .Property(e => e.UnitPrice)
            .HasConversion<double>();
        modelBuilder.Entity<OrderItem>()
            .Property(e => e.UnitPrice)
            .HasPrecision(19, 4);
        modelBuilder.Entity<OrderItem>()
            .HasOne(x => x.Product);
        modelBuilder.Entity<Order>()
            .Property(e => e.TotalAmount)
            .HasConversion<double>();
        modelBuilder.Entity<Order>()
            .Property(e => e.TotalAmount)
            .HasPrecision(19, 4);
        modelBuilder.Entity<Order>()
            .HasOne(x => x.Customer);
        modelBuilder.Entity<Order>()
            .HasMany(x => x.OrderItem);
        modelBuilder.Entity<Product>()
            .Property(e => e.Price)
            .HasConversion<double>();
        modelBuilder.Entity<Product>()
            .Property(e => e.Price)
            .HasPrecision(19, 4);
        modelBuilder.Entity<Product>()
            .HasMany(x => x.Category);
    }
}
