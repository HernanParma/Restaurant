using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Queries;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Queries
{
    public sealed class OrderQuery : IOrderQuery
    {
        private readonly AppDbContext _db;
        public OrderQuery(AppDbContext db) => _db = db;

        public async Task<IReadOnlyList<OrderListDto>> SearchAsync(OrderFilterQuery filter, CancellationToken ct = default)
        {
            var q = _db.Orders.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.DeliveryTo))
            {
                var term = filter.DeliveryTo.Trim().ToLower();
                q = q.Where(o => o.DeliveryTo != null && o.DeliveryTo.ToLower().Contains(term));
            }

            DateTime? from = filter.From?.UtcDateTime;
            DateTime? toExcl = filter.To?.UtcDateTime.AddDays(1);

            if (from.HasValue)
                q = q.Where(o => o.CreateDate >= from.Value);
            if (toExcl.HasValue)
                q = q.Where(o => o.CreateDate < toExcl.Value);

            if (filter.Status.HasValue)
                q = q.Where(o => o.OverallStatus == filter.Status.Value);

            return await q
                .OrderByDescending(o => o.CreateDate)
                .Select(o => new OrderListDto
                {
                    OrderNumber = o.OrderId,
                    TotalAmount = o.Price,
                    DeliveryTo = o.DeliveryTo,
                    Notes = o.Notes,
                    Status = new StatusLiteDto
                    {
                        Id = o.OverallStatuses.Id,
                        Name = o.OverallStatuses.Name
                    },
                    DeliveryType = new DeliveryTypeLiteDto
                    {
                        Id = o.DeliveryTypes.Id,
                        Name = o.DeliveryTypes.Name
                    },
                    Items = o.Items.Select(i => new OrderItemListDto
                    {
                        Id = i.OrderItemId,
                        Quantity = i.Quantity,
                        Notes = i.Notes,
                        Status = new StatusLiteDto
                        {
                            Id = i.Status.Id,
                            Name = i.Status.Name
                        },
                        Dish = new DishLiteDto
                        {
                            Id = i.Dish.DishId,
                            Name = i.Dish.Name,
                            Image = i.Dish.ImageUrl
                        }
                    }).ToList(),
                    CreatedAt = o.CreateDate,
                    UpdatedAt = o.UpdateDate
                })
                .ToListAsync(ct);
        }

        public async Task<(bool exists, int statusId)> GetExistsAndStatusAsync(long orderId, CancellationToken ct = default)
        {
            var row = await _db.Orders
                .AsNoTracking()
                .Where(o => o.OrderId == orderId)
                .Select(o => new { o.OrderId, o.OverallStatus })
                .FirstOrDefaultAsync(ct);

            return row is null ? (false, 0) : (true, row.OverallStatus);
        }

        public async Task<OrderListDto?> GetByIdAsync(long orderId, CancellationToken ct = default)
        {
            return await _db.Orders
                .AsNoTracking()
                .Where(o => o.OrderId == orderId)
                .Select(o => new OrderListDto
                {
                    OrderNumber = o.OrderId,
                    TotalAmount = o.Price,
                    DeliveryTo = o.DeliveryTo,
                    Notes = o.Notes,
                    Status = new StatusLiteDto
                    {
                        Id = o.OverallStatuses.Id,
                        Name = o.OverallStatuses.Name
                    },
                    DeliveryType = new DeliveryTypeLiteDto
                    {
                        Id = o.DeliveryTypes.Id,
                        Name = o.DeliveryTypes.Name
                    },
                    Items = o.Items.Select(i => new OrderItemListDto
                    {
                        Id = i.OrderItemId,
                        Quantity = i.Quantity,
                        Notes = i.Notes,
                        Status = new StatusLiteDto
                        {
                            Id = i.Status.Id,
                            Name = i.Status.Name
                        },
                        Dish = new DishLiteDto
                        {
                            Id = i.Dish.DishId,
                            Name = i.Dish.Name,
                            Image = i.Dish.ImageUrl
                        }
                    }).ToList(),
                    CreatedAt = o.CreateDate,
                    UpdatedAt = o.UpdateDate
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<(bool orderExists, bool itemExists, int currentItemStatusId)> GetItemStatusAsync(long orderId, long itemId, CancellationToken ct = default)
        {
            var item = await _db.OrderItems
                .AsNoTracking()
                .Where(i => i.OrderId == orderId && i.OrderItemId == itemId)
                .Select(i => new { i.OrderId, i.OrderItemId, i.StatusId })
                .FirstOrDefaultAsync(ct);

            if (item is null)
            {
                var orderExists = await _db.Orders.AsNoTracking().AnyAsync(o => o.OrderId == orderId, ct);
                return (orderExists, false, 0);
            }

            return (true, true, item.StatusId);
        }

        public async Task<IReadOnlyList<int>> GetOrderItemStatusIdsAsync(long orderId, CancellationToken ct = default)
        {
            return await _db.OrderItems
                .AsNoTracking()
                .Where(i => i.OrderId == orderId)
                .Select(i => i.StatusId)
                .ToListAsync(ct);
        }
    }
}
