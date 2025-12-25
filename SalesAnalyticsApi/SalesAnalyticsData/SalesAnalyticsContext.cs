using Microsoft.EntityFrameworkCore;
using SalesAnalyticsData.Models;

namespace SalesAnalyticsData
{
    public class SalesAnalyticsContext : DbContext
    {
        public SalesAnalyticsContext(DbContextOptions<SalesAnalyticsContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProductId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.HasIndex(e => e.ProductId).IsUnique();
            });

            // Customer
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CustomerId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).HasMaxLength(255);
                entity.Property(e => e.Email).HasMaxLength(255);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.HasIndex(e => e.CustomerId).IsUnique();
            });

            // Order
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OrderId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Region).HasMaxLength(100);
                entity.Property(e => e.PaymentMethod).HasMaxLength(100);
                entity.HasIndex(e => e.OrderId).IsUnique();

                entity.HasOne(e => e.Customer)
                    .WithMany()
                    .HasForeignKey(e => e.CustomerIdRef)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // OrderItem
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Quantity).IsRequired();
                entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
                entity.Property(e => e.Discount).HasPrecision(18, 2);
                entity.Property(e => e.ShippingCost).HasPrecision(18, 2);

                entity.HasOne(e => e.Order)
                    .WithMany(o => o.Items)
                    .HasForeignKey(e => e.OrderRefId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Product)
                    .WithMany()
                    .HasForeignKey(e => e.ProductRefId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
