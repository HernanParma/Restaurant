using Application.Dishes.Dtos;
using Application.Queries;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Queries
{
    public class DishQuery : IDishQuery
    {
        private readonly AppDbContext _db;
        public DishQuery(AppDbContext db) => _db = db;
        public async Task<DishResponseDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var dish = await _db.Dishes
                .Include(d => d.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DishId == id, ct);
            if (dish is null) return null;
            return new DishResponseDto
            {
                Id = dish.DishId,
                Name = dish.Name,
                Description = dish.Description,
                Price = dish.Price,
                Category = new CategoryDto { Id = dish.Category!.Id, Name = dish.Category!.Name },
                Image = dish.ImageUrl,
                IsActive = dish.Available,
                CreatedAt = dish.CreateDate,
                UpdatedAt = dish.UpdateDate
            };
        }
        public async Task<IReadOnlyList<DishResponseDto>> SearchAsync(DishFilterQuery q, CancellationToken ct = default)
        {
            var query = _db.Dishes.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(q.Name))
                query = query.Where(d => EF.Functions.Like(d.Name, $"%{q.Name.Trim()}%"));
            if (q.Category.HasValue)
                query = query.Where(d => d.CategoryId == q.Category.Value);
            if (q.OnlyActive.HasValue && q.OnlyActive.Value)
                query = query.Where(d => d.Available);
            if (q.SortByPrice.HasValue)
                query = q.SortByPrice.Value == PriceSort.desc
                    ? query.OrderByDescending(d => d.Price)
                    : query.OrderBy(d => d.Price);
            else
                query = query.OrderBy(d => d.Price);
            query = query.Include(d => d.Category);
            return await query.Select(d => new DishResponseDto
            {
                Id = d.DishId,
                Name = d.Name,
                Description = d.Description,
                Price = d.Price,
                Category = new CategoryDto { Id = d.Category!.Id, Name = d.Category!.Name },
                Image = d.ImageUrl,
                IsActive = d.Available,
                CreatedAt = d.CreateDate,
                UpdatedAt = d.UpdateDate
            }).ToListAsync(ct);
        }
    }
}