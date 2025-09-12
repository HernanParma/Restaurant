using Application.Dtos;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Application.Mappers;

namespace Infrastructure.Commands
{
    public class DishCommand : IDishCommand
    {
        private readonly AppDbContext _db;
        public DishCommand(AppDbContext db) => _db = db;

        public async Task<DishResponseDto> CreateAsync(DishCreateDto dto, CancellationToken ct = default)
        {
            if (dto.Price <= 0) throw new ArgumentException("El precio debe ser mayor a 0.");
            var name = dto.Name?.Trim();
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("El nombre es obligatorio.");

            var catExists = await _db.Categories.AnyAsync(c => c.Id == dto.Category, ct);
            if (!catExists) throw new KeyNotFoundException("La categoría no existe.");

            var duplicate = await _db.Dishes.AnyAsync(d => d.Name == name, ct);
            if (duplicate) throw new InvalidOperationException("Ya existe un plato con ese nombre.");

            var entity = new Dish
            {
                DishId = Guid.NewGuid(),
                Name = name!,
                Description = dto.Description,
                Price = dto.Price,
                CategoryId = dto.Category,
                ImageUrl = dto.Image,
                Available = dto.IsActive,
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };
            await _db.Dishes.AddAsync(entity, ct);
            await _db.SaveChangesAsync(ct);
            return await _db.Dishes
            .AsNoTracking()
            .Where(d => d.DishId == entity.DishId)
            .Select(DishMapper.ToDtoProjection())
            .FirstAsync(ct);
        }
        public async Task<DishResponseDto> UpdateAsync(Guid id, DishUpdateDto dto, CancellationToken ct = default)
        {
            var dish = await _db.Dishes.FirstOrDefaultAsync(x => x.DishId == id, ct);
            if (dish is null) throw new KeyNotFoundException("El plato no existe.");

            if (dto.Price.HasValue && dto.Price.Value <= 0)
                throw new ArgumentException("El precio debe ser mayor a 0.");

            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                var newName = dto.Name.Trim();
                var isNameChanging = !newName.Equals(dish.Name, StringComparison.OrdinalIgnoreCase);

                if (isNameChanging)
                {
                    var duplicate = await _db.Dishes.AnyAsync(
                        x => x.DishId != id && x.Name == newName, ct);

                    if (duplicate)
                        throw new InvalidOperationException("Ya existe un plato con ese nombre.");

                    dish.Name = newName;
                }
            }
            if (dto.Category.HasValue)
            {
                var catExists = await _db.Categories.AnyAsync(x => x.Id == dto.Category.Value, ct);
                if (!catExists) throw new KeyNotFoundException("La categoría no existe.");
                dish.CategoryId = dto.Category.Value;
            }
            if (dto.Description is not null) dish.Description = dto.Description;
            if (dto.Price.HasValue) dish.Price = dto.Price.Value;
            if (dto.Image is not null) dish.ImageUrl = dto.Image;
            if (dto.IsActive.HasValue) dish.Available = dto.IsActive.Value;

            dish.UpdateDate = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return await _db.Dishes
            .AsNoTracking()
            .Where(d => d.DishId == dish.DishId)
            .Select(DishMapper.ToDtoProjection())
            .FirstAsync(ct);
        }
    }
}