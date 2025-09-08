using Application.Dishes.Command.CreateDish;
using Application.Dishes.Dtos;
using Domain.Entities;
using Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Dishes.Command
{
        public sealed class CreateDishHandler : ICreateDishHandler
        {
            private readonly AppDbContext _db;

            public CreateDishHandler(AppDbContext db) => _db = db;

            public async Task<DishResponseDto> HandleAsync(CreateDishCommand command, CancellationToken ct = default)
            {
                var dto = command.Dto;

                if (dto.Price <= 0)
                    throw new ArgumentException("El precio debe ser mayor a cero");
                var nameNormalized = (dto.Name ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(nameNormalized))
                    throw new ArgumentException("El nombre es obligatorio");
                var category = await _db.Categories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == dto.CategoryId, ct);
                if (category is null)
                    throw new KeyNotFoundException("La categoría no existe");
                var exists = await _db.Dishes
                    .AnyAsync(d => d.Name.ToUpper() == nameNormalized.ToUpper(), ct);
                if (exists)
                    throw new InvalidOperationException("Ya existe un plato con ese nombre");
                var entity = new Dish
                {
                    DishId = Guid.NewGuid(),
                    Name = nameNormalized,
                    Description = dto.Description?.Trim() ?? string.Empty,
                    Price = dto.Price,
                    CategoryId = dto.CategoryId,
                    ImageUrl = dto.Image?.Trim(),
                    Available = true,
                    CreateDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow
                };
                _db.Dishes.Add(entity);
                await _db.SaveChangesAsync(ct);
                return new DishResponseDto
                {
                    Id = entity.DishId,
                    Name = entity.Name,
                    Description = entity.Description,
                    Price = entity.Price,
                    Category = new { id = category.Id, name = category.Name },
                    Image = entity.ImageUrl,
                    IsActive = entity.Available,
                    CreatedAt = entity.CreateDate,
                    UpdatedAt = entity.UpdateDate
                };
            }
        }
 }