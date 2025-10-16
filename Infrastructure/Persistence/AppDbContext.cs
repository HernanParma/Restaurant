using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<DeliveryType> DeliveryTypes { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<Category> Categories { get; set; }
           protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Category 
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Category");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(25);
                entity.Property(c => c.Description).HasMaxLength(255);
                entity.Property(c => c.Order).HasColumnName("Order"); 

                // relación con Dish
                entity.HasMany(c => c.Dishes)
                      .WithOne(d => d.Category)
                      .HasForeignKey(d => d.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // DeliveryTypes
            modelBuilder.Entity<DeliveryType>(entity =>
            {
                entity.ToTable("DeliveryType");
                entity.HasKey(dt => dt.Id);
                entity.Property(dt => dt.Name).IsRequired().HasMaxLength(25);

                // relación con Order (FK: Order.DeliveryType)
                entity.HasMany(dt => dt.Orders)
                      .WithOne(o => o.DeliveryTypes)          
                      .HasForeignKey(o => o.DeliveryType)     
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Status
            modelBuilder.Entity<Status>(entity =>
            {
                entity.ToTable("Status");
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Name).IsRequired().HasMaxLength(25);

                // relación con Order (FK: Order.OverallStatus)
                entity.HasMany(s => s.Orders)
                      .WithOne(o => o.OverallStatuses)        
                      .HasForeignKey(o => o.OverallStatus)    
                      .OnDelete(DeleteBehavior.Restrict);

                // relación con OrderItem
                entity.HasMany(s => s.OrderItems)
                      .WithOne(oi => oi.Status)
                      .HasForeignKey(oi => oi.StatusId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Dish
            modelBuilder.Entity<Dish>(entity =>
            {
                entity.ToTable("Dish");
                entity.HasKey(d => d.DishId);
                entity.Property(d => d.Name).IsRequired().HasMaxLength(255);
                entity.Property(d => d.Description).IsRequired(false); ;
                entity.Property(d => d.Price).HasColumnType("decimal(18,2)");
                entity.Property(d => d.Available).IsRequired();
                entity.Property(d => d.ImageUrl).IsRequired(false); ;
                entity.Property(d => d.CreateDate).IsRequired();
                entity.Property(d => d.UpdateDate);

                // FK CategoryId -> columna "Category"
                entity.Property(d => d.CategoryId).HasColumnName("Category");

                // relación con OrderItem
                entity.HasMany(d => d.OrderItems)
                      .WithOne(oi => oi.Dish)
                      .HasForeignKey(oi => oi.DishId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            // Order
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Order");
                entity.HasKey(o => o.OrderId);
                entity.Property(o => o.DeliveryTo).IsRequired().HasMaxLength(255);
                entity.Property(o => o.Price).HasColumnType("decimal(18,2)");
                entity.Property(o => o.CreateDate).IsRequired();
                entity.Property(o => o.UpdateDate);
               
                //relacion con DeliveryTypes
                entity.HasOne(o => o.DeliveryTypes)
                      .WithMany(dt => dt.Orders)
                      .HasForeignKey(o => o.DeliveryType)
                      .OnDelete(DeleteBehavior.Restrict);
                //relacion con Status
                entity.HasOne(o => o.OverallStatuses)
                      .WithMany(s => s.Orders)
                      .HasForeignKey(o => o.OverallStatus)
                      .OnDelete(DeleteBehavior.Restrict);
                //relacion con OrderItem
                entity.HasMany(o => o.Items)
                      .WithOne(oi => oi.Order)
                      .HasForeignKey(oi => oi.OrderId)
                      .OnDelete(DeleteBehavior.Cascade); 
            });
            // OrderItem
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("OrderItem");
                entity.HasKey(oi => oi.OrderItemId);
                entity.Property(oi => oi.Quantity).IsRequired();
                entity.Property(oi => oi.Notes);
                entity.Property(oi => oi.CreateDate).IsRequired();
                //relacion con Order
                entity.HasOne(oi => oi.Order)
                      .WithMany(o => o.Items)
                      .HasForeignKey(oi => oi.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
                //relacion con Dish
                entity.HasOne(oi => oi.Dish)
                      .WithMany(d => d.OrderItems)
                      .HasForeignKey(oi => oi.DishId)
                      .OnDelete(DeleteBehavior.Restrict);
                //relacion con Status
                entity.HasOne(oi => oi.Status)
                      .WithMany(s => s.OrderItems)
                      .HasForeignKey(oi => oi.StatusId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}