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
            var now = DateTime.UtcNow;

            var entity = new Dish
            {
                DishId = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                CategoryId = dto.Category,
                Available = dto.IsActive,
                ImageUrl = dto.Image,
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };

            _db.Dishes.Add(entity);
            await _db.SaveChangesAsync(ct);

            var cat = await _db.Categories.AsNoTracking()
                .Where(c => c.Id == entity.CategoryId)
                .Select(c => new CategoryLiteDto { Id = c.Id, Name = c.Name })
                .FirstAsync(ct);

            return new DishResponseDto
            {
                Id = entity.DishId,
                Name = entity.Name,
                Description = entity.Description,
                Price = entity.Price,
                Category = cat,                 
                ImageUrl = entity.ImageUrl,
                Available = entity.Available,
                CreatedAt = entity.CreateDate,
                UpdatedAt = entity.UpdateDate
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
            entity.ImageUrl = dto.Image;
            entity.UpdateDate = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);

            var cat = await _db.Categories.AsNoTracking()
                .Where(c => c.Id == entity.CategoryId)
                .Select(c => new CategoryLiteDto { Id = c.Id, Name = c.Name })
                .FirstAsync(ct);

            return new DishResponseDto
            {
                Id = entity.DishId,
                Name = entity.Name,
                Description = entity.Description,
                Price = entity.Price,
                Category = cat,                  
                ImageUrl = entity.ImageUrl,
                Available = entity.Available,
                CreatedAt = entity.CreateDate,
                UpdatedAt = entity.UpdateDate
            };
        }

        public async Task<DishResponseDto> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _db.Dishes.FirstOrDefaultAsync(d => d.DishId == id, ct);
            if (entity is null)
                throw new KeyNotFoundException("Plato no encontrado");

            // ¿Está referenciado en alguna orden, sin importar el estado?
            var referenced = await _db.OrderItems
                .AsNoTracking()
                .AnyAsync(i => i.DishId == id, ct);

            if (referenced)
            {
               
                entity.Available = false;
                entity.UpdateDate = DateTime.UtcNow;
                await _db.SaveChangesAsync(ct);

                var cat = await _db.Categories.AsNoTracking()
                    .Where(c => c.Id == entity.CategoryId)
                    .Select(c => new CategoryLiteDto { Id = c.Id, Name = c.Name })
                    .FirstAsync(ct);

                return new DishResponseDto
                {
                    Id = entity.DishId,
                    Name = entity.Name,
                    Description = entity.Description,
                    Price = entity.Price,
                    Category = cat,
                    ImageUrl = entity.ImageUrl,
                    Available = entity.Available,
                    CreatedAt = entity.CreateDate,
                    UpdatedAt = entity.UpdateDate
                };
            }

            _db.Dishes.Remove(entity);
            await _db.SaveChangesAsync(ct);

            return new DishResponseDto
            {
                Id = entity.DishId,
                Name = entity.Name,
                Description = entity.Description,
                Price = entity.Price,
                Category = await _db.Categories.AsNoTracking()
                    .Where(c => c.Id == entity.CategoryId)
                    .Select(c => new CategoryLiteDto { Id = c.Id, Name = c.Name })
                    .FirstAsync(ct),
                ImageUrl = entity.ImageUrl,
                Available = entity.Available,
                CreatedAt = entity.CreateDate,
                UpdatedAt = entity.UpdateDate
            };
        }
    }
}
