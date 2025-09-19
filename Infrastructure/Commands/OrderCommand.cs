using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Commands
{
    public sealed class OrderCommand : IOrderCommand
    {
        private readonly AppDbContext _db;
        public OrderCommand(AppDbContext db) => _db = db;

        public async Task<OrderCreatedResponseDto> CreateAsync(CreateOrderCommandModel model, CancellationToken ct = default)
        {
            var order = new Order
            {
                DeliveryTypeId = model.DeliveryTypeId,
                DeliveryTo = model.DeliveryTo,
                Notes = model.Notes,
                Price = model.TotalAmount,
                OverallStatusId = model.InitialStatusId,
                CreateDate = DateTime.UtcNow
            };

            // Crear items
            foreach (var it in model.Items)
            {
                order.Items.Add(new OrderItem
                {
                    DishId = it.DishId,
                    Quantity = it.Quantity,
                    //UnitPrice = it.UnitPrice,
                    Notes = it.Notes,
                    CreateDate = DateTime.UtcNow,
                    StatusId = model.InitialStatusId
                });
            }

            _db.Orders.Add(order);
            await _db.SaveChangesAsync(ct);

            return new OrderCreatedResponseDto
            {
                OrderNumber = order.OrderId,
                TotalAmount = order.Price,
                CreatedAt = order.CreateDate
            };
        }
        public async Task<OrderUpdatedResponseDto> UpdateItemsAsync(
    long orderId,
    IReadOnlyList<OrderItemToPersist> newItems,
    decimal newTotal,
    CancellationToken ct = default)
        {
            var order = await _db.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.OrderId == orderId, ct);

            if (order is null)
                throw new KeyNotFoundException("Orden no encontrada");

            // Reemplazo full de items:
            _db.OrderItems.RemoveRange(order.Items);

            foreach (var it in newItems)
            {
                order.Items.Add(new OrderItem
                {
                    DishId = it.DishId,
                    Quantity = it.Quantity,
                  //UnitPrice = it.UnitPrice,
                    Notes = it.Notes,
                    CreateDate = DateTime.UtcNow,
                    StatusId = order.OverallStatusId 
                });
            }

            order.Price = newTotal;
            order.UpdateDate = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);

            return new OrderUpdatedResponseDto
            {
                OrderNumber = order.OrderId,
                TotalAmount = order.Price,
                UpdateAt = order.UpdateDate ?? DateTime.UtcNow
            };
        }
        public async Task<OrderUpdatedResponseDto> UpdateItemStatusAsync(long orderId, long itemId, int newItemStatusId, int newOverallStatusId, CancellationToken ct = default)
        {
            var order = await _db.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.OrderId == orderId, ct);

            if (order is null)
                throw new KeyNotFoundException("Orden no encontrada");

            var item = order.Items.FirstOrDefault(i => i.OrderItemId == itemId);
            if (item is null)
                throw new KeyNotFoundException("Item no encontrado");

            item.StatusId = newItemStatusId;
            order.OverallStatusId = newOverallStatusId;
            order.UpdateDate = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);

            return new OrderUpdatedResponseDto
            {
                OrderNumber = order.OrderId,
                TotalAmount = order.Price,
                UpdateAt = order.UpdateDate ?? DateTime.UtcNow
            };
        }
    }
}
