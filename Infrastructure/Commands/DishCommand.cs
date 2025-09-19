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
    public sealed class DishCommand : IDishCommand
    {
        private readonly AppDbContext _db;

        public DishCommand(AppDbContext db) => _db = db;

        public async Task<DishResponseDto> CreateAsync(DishCreateDto dto, CancellationToken ct)
        {
            var entity = new Dish
            {
                DishId = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                CategoryId = dto.Category,
                Available = dto.IsActive
            };

            _db.Dishes.Add(entity);
            await _db.SaveChangesAsync(ct);

            return new DishResponseDto
            {
                Id = entity.DishId,
                Name = entity.Name,
                Description = entity.Description,
                Price = entity.Price,
                Category = new CategoryDto   
                {
                    Id = entity.CategoryId,
                    Name = entity.Category?.Name ?? string.Empty,
                    Description = entity.Category?.Description,
                    Order = entity.Category?.Order ?? 0
                },
                IsActive = entity.Available
            };
        }
        public async Task<DishResponseDto> UpdateAsync(Guid id, DishUpdateDto dto, CancellationToken ct)
        {
            var entity = await _db.Dishes.FirstOrDefaultAsync(d => d.DishId == id, ct);
            if (entity is null)
                throw new KeyNotFoundException("Dish no encontrado."); 

            entity.Name = dto.Name;
            entity.Description = dto.Description;
            entity.Price = dto.Price;
            entity.CategoryId = dto.Category;
            entity.Available = dto.IsActive;
            entity.UpdateDate = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);

            var cat = await _db.Categories.AsNoTracking()
                .Where(c => c.Id == entity.CategoryId)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Order = c.Order
                })
                .FirstOrDefaultAsync(ct);

            return new DishResponseDto
            {
                Id = entity.DishId,
                Name = entity.Name,
                Description = entity.Description,
                Price = entity.Price,
                Category = cat ?? new CategoryDto { Id = entity.CategoryId, Name = string.Empty, Description = null, Order = 0 },
                IsActive = entity.Available,
                CreatedAt = entity.CreateDate,
                UpdatedAt = entity.UpdateDate
            };
        }
        public async Task<DishResponseDto> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _db.Dishes.FirstOrDefaultAsync(d => d.DishId == id, ct);
            if (entity is null)
                throw new KeyNotFoundException("Plato no encontrado");

            _db.Dishes.Remove(entity);
            await _db.SaveChangesAsync(ct);
            return new DishResponseDto
            {
                Id = entity.DishId,
                Name = entity.Name,
                Description = entity.Description,
                Price = entity.Price,
                Category = new CategoryDto { Id = entity.CategoryId },
                IsActive = entity.Available,
                CreatedAt = entity.CreateDate,
                UpdatedAt = entity.UpdateDate
            };
        }
    }

}