using Application.Dishes.Dtos;
using Application.Queries;
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
        public DishQuery(AppDbContext context)
        {
            _db = context;
        }
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
                Category = new { id = dish.Category!.Id, name = dish.Category!.Name },
                Image = dish.ImageUrl,
                IsActive = dish.Available,
                CreatedAt = dish.CreateDate,
                UpdatedAt = dish.UpdateDate
            };
        }
    }
}