using Microsoft.EntityFrameworkCore;
using EHandelDb.Models;
using System.IO;

namespace EHandelDb 
{
    public class ShopContext : DbContext
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderRow> OrderRows { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dbPath = Path.Combine(AppContext.BaseDirectory, "shop.db");
            optionsBuilder.UseSqlite($"Filename={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Customer Konfiguration
            modelBuilder.Entity<Customer>(e =>
            {
                e.HasKey(c => c.CustomerId);
                e.Property(c => c.CustomerName).IsRequired().HasMaxLength(100);
                e.Property(c => c.Email).IsRequired().HasMaxLength(100);
                e.Property(c => c.City).HasMaxLength(100);

                e.HasIndex(c => c.Email).IsUnique(); 
            });

            // Category Konfiguration
            modelBuilder.Entity<Category>(e =>
            {
                e.HasKey(g => g.CategoryId);
                e.Property(g => g.CategoryName).IsRequired().HasMaxLength(100);
                e.Property(g => g.CategoryDescription).HasMaxLength(150); 
            });

            // Product Konfiguration
            modelBuilder.Entity<Product>(e =>
            {
                e.HasKey(p => p.ProductId);
                e.Property(p => p.ProductName).IsRequired().HasMaxLength(100);
                e.Property(p => p.Price).IsRequired();
 
                e.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict); 
            });
            
            // Order Konfiguration
            modelBuilder.Entity<Order>(e =>
            {
                e.HasKey(o => o.OrderId);
                e.Property(o => o.OrderDate).IsRequired();
                e.Property(o => o.Status).IsRequired().HasMaxLength(100);
                
                e.HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
            });
            
            // OrderRow Konfiguration
            modelBuilder.Entity<OrderRow>(e =>
            {
                e.HasKey(r => r.OrderRowId);
                e.Property(r => r.Quantity).IsRequired();
                e.Property(r => r.UnitPrice).IsRequired();
                
                e.HasOne(r => r.Order)
                .WithMany(o => o.OrderRows)
                .HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(r => r.Product)
                .WithMany(p => p.OrderRows)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}