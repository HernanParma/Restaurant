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
                entity.Property(a => a.Name).HasMaxLength(25);
                // relacion uno a muchos
                entity.HasMany<ApprovalRule>(a => a.ApprovalRules).WithOne(ar => ar.Areas).HasForeignKey(ar => ar.Area);
                entity.HasMany<ProjectProposal>(a => a.ProjectProposals).WithOne(pp => pp.Areas).HasForeignKey(pp => pp.Area);
            });

            //DeliveryType
            //Dish
            //Order
            //OrderItem
            //Status
        }


        }
}
