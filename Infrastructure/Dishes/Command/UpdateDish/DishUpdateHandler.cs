using Application.Dishes.Command.UpdateDish;
using Application.Dishes.Dtos;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Application.Dishes.Command.UpdateDish.DishUpdateCommand;

namespace Infrastructure.Dishes.Command.UpdateDish
{
    public class DishUpdateHandler : IDishUpdateHandler
    {
        private readonly AppDbContext _db;

        public DishUpdateHandler(AppDbContext db) => _db = db;

        public async Task<DishResponseDto> HandleAsync(UpdateDishCommand command, CancellationToken ct = default)
        {
            var id = command.Id;
            var dto = command.Dto;

            var dish = await _db.Dishes.FirstOrDefaultAsync(x => x.DishId == id, ct);
            if (dish is null) throw new KeyNotFoundException("El plato no existe.");

            if (dto.Price.HasValue && dto.Price.Value <= 0)
                throw new ArgumentException("El precio debe ser mayor a 0.");

            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                var duplicate = await _db.Dishes.AnyAsync(
                    x => x.DishId != id && x.Name == dto.Name!, ct);
                if (duplicate)
                    throw new InvalidOperationException("Ya existe un plato con ese nombre.");
            }

            if (dto.Category.HasValue)
            {
                var catExists = await _db.Categories.AnyAsync(x => x.Id == dto.Category.Value, ct);
                if (!catExists) throw new KeyNotFoundException("La categoría no existe.");
                dish.CategoryId = dto.Category.Value;
            }
            if (!string.IsNullOrWhiteSpace(dto.Name)) dish.Name = dto.Name!;
            if (dto.Description is not null) dish.Description = dto.Description;
            if (dto.Price.HasValue) dish.Price = dto.Price.Value;
            if (dto.Image is not null) dish.ImageUrl = dto.Image;
            if (dto.IsActive.HasValue) dish.Available = dto.IsActive.Value;
            dish.UpdateDate = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            await _db.Entry(dish).Reference(d => d.Category).LoadAsync(ct);

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
    }
}