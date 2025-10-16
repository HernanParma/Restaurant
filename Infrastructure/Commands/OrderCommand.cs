using System;
using System.Collections.Generic;
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

        private const int BlockEditFromStatusId = 2;

        private static bool IsValidTransition(int from, int to) => (from, to) switch
        {
            (0, 1) => true,
            (1, 2) => true,
            (2, 3) => true,
            (3, 4) => true,
            (4, 5) => true,
            (_, 6) => true,
            _ => false
        };

        public async Task<OrderCreatedResponseDto> CreateAsync(CreateOrderCommandModel model, CancellationToken ct = default)
        {
            var order = new Order
            {
                DeliveryType = model.DeliveryTypeId,
                DeliveryTo = model.DeliveryTo,
                Notes = model.Notes,
                OverallStatus = model.InitialStatusId,
                CreateDate = DateTime.UtcNow
            };

            foreach (var it in model.Items)
            {
                order.Items.Add(new OrderItem
                {
                    DishId = it.DishId,
                    Quantity = it.Quantity,
                    Notes = it.Notes,
                    CreateDate = DateTime.UtcNow,
                    StatusId = model.InitialStatusId
                });
            }

            if (order.Items.Count == 0)
                throw new InvalidOperationException("La orden debe contener al menos un ítem.");

            _db.Orders.Add(order);
            order.Price = await RecalculateTotalAsync(order, ct);
            await _db.SaveChangesAsync(ct);

            return new OrderCreatedResponseDto
            {
                OrderNumber = order.OrderId,
                TotalAmount = order.Price,
                CreatedAt = order.CreateDate
            };
        }
        public async Task<OrderUpdatedResponseDto> PatchAsync(long orderId, OrderPatchDto dto, CancellationToken ct = default)
        {
            var order = await _db.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.OrderId == orderId, ct)
                ?? throw new KeyNotFoundException("Orden no encontrada");

            if (order.OverallStatus >= 2)
                throw new InvalidOperationException("La orden no admite modificaciones a partir del estado 2.");

            if (!string.IsNullOrWhiteSpace(dto.DeliveryTo))
                order.DeliveryTo = dto.DeliveryTo!;
            if (dto.Notes is not null)
                order.Notes = dto.Notes;

            if (dto.Items is { Count: > 0 })
            {
                foreach (var op in dto.Items)
                {
                    var kind = op.Op.Trim().ToLowerInvariant();
                    switch (kind)
                    {
                        case "add":
                            {
                                if (op.DishId is null) throw new InvalidOperationException("DishId requerido para add.");
                                var qty = (op.Quantity ?? 1);
                                if (qty <= 0) throw new InvalidOperationException("Cantidad inválida.");
                                var dishId = op.DishId.Value;
                                order.Items.Add(new OrderItem
                                {
                                    DishId = dishId,
                                    Quantity = qty,
                                    Notes = op.Notes,
                                    CreateDate = DateTime.UtcNow,
                                    StatusId = order.OverallStatus
                                });
                                break;
                            }
                        case "update":
                            {
                                OrderItem? target = null;
                                if (op.OrderItemId is long oi)
                                    target = order.Items.FirstOrDefault(i => i.OrderItemId == oi);
                                else if (op.DishId is Guid gd)
                                    target = order.Items.FirstOrDefault(i => i.DishId == gd);

                                if (target is null) throw new KeyNotFoundException("Item no encontrado");

                                if (op.Quantity is int q)
                                {
                                    if (q <= 0) throw new InvalidOperationException("Cantidad inválida.");
                                    target.Quantity = q;
                                }
                                if (op.Notes is not null) target.Notes = op.Notes;
                                break;
                            }
                        case "remove":
                            {
                                OrderItem? target = null;
                                if (op.OrderItemId is long oi)
                                    target = order.Items.FirstOrDefault(i => i.OrderItemId == oi);
                                else if (op.DishId is Guid gd)
                                    target = order.Items.FirstOrDefault(i => i.DishId == gd);

                                if (target is null) throw new KeyNotFoundException("Item no encontrado");
                                _db.OrderItems.Remove(target);
                                break;
                            }
                        default:
                            throw new InvalidOperationException($"Operación inválida: {op.Op}");
                    }
                }
            }

            if (!order.Items.Any())
                throw new InvalidOperationException("La orden debe contener al menos un ítem.");

            order.Price = await RecalculateTotalAsync(order, ct);
            order.UpdateDate = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);

            return new OrderUpdatedResponseDto
            {
                OrderNumber = order.OrderId,
                TotalAmount = order.Price,
                UpdateAt = order.UpdateDate ?? DateTime.UtcNow
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

            if (order.OverallStatus >= BlockEditFromStatusId)
                throw new InvalidOperationException("La orden no admite modificaciones de ítems a partir del estado 2.");

            _db.OrderItems.RemoveRange(order.Items);

            foreach (var it in newItems)
            {
                order.Items.Add(new OrderItem
                {
                    DishId = it.DishId,
                    Quantity = it.Quantity,
                    Notes = it.Notes,
                    CreateDate = DateTime.UtcNow,
                    StatusId = order.OverallStatus
                });
            }

            if (order.Items.Count == 0)
                throw new InvalidOperationException("La orden debe contener al menos un ítem.");

            order.Price = await RecalculateTotalAsync(order, ct);
            order.UpdateDate = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);

            return new OrderUpdatedResponseDto
            {
                OrderNumber = order.OrderId,
                TotalAmount = order.Price,
                UpdateAt = order.UpdateDate ?? DateTime.UtcNow
            };
        }

        public async Task<OrderUpdatedResponseDto> UpdateItemStatusAsync(
    long orderId,
    long itemId,
    int newItemStatusId,
    int _ /* ignorado: newOverallStatusId */,
    CancellationToken ct = default)
        {
            var order = await _db.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.OrderId == orderId, ct)
                ?? throw new KeyNotFoundException("Orden no encontrada");

            var item = order.Items.FirstOrDefault(i => i.OrderItemId == itemId)
                       ?? throw new KeyNotFoundException("Item no encontrado");

            // 1) Actualizá el estado del ítem
            item.StatusId = newItemStatusId;

            // 2) Recalculá el estado global como el "más atrasado" (mínimo de los ítems)
            var recalculatedOverall = order.Items.Min(i => i.StatusId);

            // 3) No permitas retrocesos del global (por seguridad)
            if (recalculatedOverall < order.OverallStatus)
            {
                // si no querés lanzar, podés simplemente no cambiarlo
                throw new InvalidOperationException(
                    $"No se permite retroceder el estado global de {order.OverallStatus} a {recalculatedOverall}.");
            }

            // 4) Avance global (monótono). Si querés validar saltos, podés hacerlo acá,
            // pero en agregación por ítems suele permitirse el salto directo.
            order.OverallStatus = recalculatedOverall;

            order.UpdateDate = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);

            return new OrderUpdatedResponseDto
            {
                OrderNumber = order.OrderId,
                TotalAmount = order.Price,
                UpdateAt = order.UpdateDate ?? DateTime.UtcNow
            };
        }



        private async Task<decimal> RecalculateTotalAsync(Order order, CancellationToken ct)
        {
            var dishIds = order.Items.Select(i => i.DishId).Distinct().ToList();

            var prices = await _db.Dishes
                .Where(d => dishIds.Contains(d.DishId))
                .Select(d => new { d.DishId, d.Price })
                .ToListAsync(ct);

            var priceMap = prices.ToDictionary(x => x.DishId, x => x.Price);

            var missing = dishIds.Where(id => !priceMap.ContainsKey(id)).ToList();
            if (missing.Count > 0)
                throw new InvalidOperationException($"Hay ítems con Dish inexistente: {string.Join(", ", missing)}");

            var total = order.Items.Sum(i => priceMap[i.DishId] * i.Quantity);

            return total;
        }
    }
}
