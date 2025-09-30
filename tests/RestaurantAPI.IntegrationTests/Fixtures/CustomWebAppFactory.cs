using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

public class CustomWebAppFactory : WebApplicationFactory<Program>
{
    private DbConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // 👉 activa el if (!Testing) de Program.cs
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // 1) Quitar TODAS las registraciones previas de AppDbContext
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            services.RemoveAll(typeof(AppDbContext));
            services.RemoveAll<IDbContextFactory<AppDbContext>>();
            services.RemoveAll<DbContextOptions>();

            // 2) Crear UNA conexión SQLite en memoria y mantenerla abierta
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            // 3) Registrar AppDbContext con Sqlite usando la MISMA conexión
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            // 4) Construir provider, crear esquema y SEED de datos
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Crea las tablas según tu modelo
            db.Database.EnsureCreated();

            // --- SEEDS ------------------------------------------------------

            // Categorías
            if (!db.Categories.Any())
            {
                db.Categories.AddRange(
                    new Category { Id = 1, Name = "Pizzas" },
                    new Category { Id = 2, Name = "Bebidas" }
                );
            }

            // Delivery Types (incluye el Id=3 que usan los tests)
            if (!db.DeliveryTypes.Any())
            {
                db.DeliveryTypes.AddRange(
                    new DeliveryType { Id = 1, Name = "Local" },
                    new DeliveryType { Id = 2, Name = "Delivery" },
                    new DeliveryType { Id = 3, Name = "TakeAway" } // ← faltaba
                );
            }

            // Status (los ids que usan los tests)
            if (!db.Statuses.Any())
            {
                db.Statuses.AddRange(
                    new Status { Id = 1, Name = "Pending" },
                    new Status { Id = 2, Name = "InProgress" },
                    new Status { Id = 3, Name = "Completed" }
                );
            }

            // Platos: al menos 2 con precios distintos para tests de ordenamiento/filtrado
            if (db.Dishes.Count() < 2)
            {
                db.Dishes.AddRange(
                    new Dish
                    {
                        DishId = Guid.NewGuid(),
                        Name = "Muzzarella",
                        Description = "Clásica",
                        Price = 5000m,
                        CategoryId = 1,
                        Available = true,
                        CreateDate = DateTime.UtcNow
                    },
                    new Dish
                    {
                        DishId = Guid.NewGuid(),
                        Name = "Napolitana",
                        Description = "Con tomate y ajo",
                        Price = 6500m,
                        CategoryId = 1,
                        Available = true,
                        CreateDate = DateTime.UtcNow
                    }
                );
            }

            db.SaveChanges();

            // Orden con 1 ítem (para GET by id = 200 y delete dish = 409)
            if (!db.Orders.Any())
            {
                var dish = db.Dishes.First();
                db.Orders.Add(new Order
                {
                    DeliveryTypeId = 1,
                    DeliveryTo = "Mesa 12",
                    Price = dish.Price,
                    OverallStatusId = 1, // Pending
                    CreateDate = DateTime.UtcNow,
                    Items = new List<OrderItem>
        {
            new OrderItem
            {
                DishId = dish.DishId,
                Quantity = 1,
                StatusId = 1, // Pending
                CreateDate = DateTime.UtcNow
            }
        }
                });
                db.SaveChanges();
            }
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection?.Dispose();
        _connection = null;
    }
}
