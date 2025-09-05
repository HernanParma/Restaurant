using Application.Dishes;
using Application.Dishes.Dtos;
using Application.Queries;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Dishes
{
    public class DishService : IDishService
    {
        private readonly AppDbContext _db;
        private readonly IDishQuery _dishQuery;
        public DishService( AppDbContext db, IDishQuery dishQuery)
        {
            _db = db;
            _dishQuery = dishQuery;
        }
        public async Task<DishResponseDto> CreateAsync(DishCreateDto dto, CancellationToken ct = default)
        {
            // 1) Precio > 0
            if (dto.Price <= 0)
                throw new ArgumentException("El precio debe ser mayor a cero");

            // 2) Categoría existe (DTO: Category)
            var category = await _db.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == dto.CategoryId, ct);

            if (category is null)
                throw new KeyNotFoundException("La categoría no existe");

            // 3) Nombre único (case-insensitive)
            var nameNormalized = (dto.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(nameNormalized))
                throw new ArgumentException("El nombre es obligatorio");

            var exists = await _db.Dishes
                .AnyAsync(d => d.Name.ToUpper() == nameNormalized.ToUpper(), ct);

            if (exists)
                throw new InvalidOperationException("Ya existe un plato con ese nombre");

            // 4) Crear entidad
            var entity = new Dish
            {
                DishId = Guid.NewGuid(),
                Name = nameNormalized,
                Description = dto.Description?.Trim() ?? string.Empty, // evita null en propiedad no-nullable
                Price = dto.Price,
                CategoryId = dto.CategoryId,
                ImageUrl = dto.Image?.Trim(),
                Available = true,
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };

            _db.Dishes.Add(entity);
            await _db.SaveChangesAsync(ct);

            // 5) Proyección a respuesta
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
        public async Task<DishResponseDto> UpdateAsync(Guid id, DishUpdateDto dto, CancellationToken ct)
        {
            var dish = await _db.Dishes.FirstOrDefaultAsync(x => x.DishId == id, ct);
            if (dish is null) throw new KeyNotFoundException("El plato no existe.");

            // Validaciones
            if (dto.Price.HasValue && dto.Price.Value <= 0)
                throw new ArgumentException("El precio debe ser mayor a 0.");

            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                var duplicate = await _db.Dishes.AnyAsync(
                    x => x.DishId != id && x.Name == dto.Name!, ct);
                if (duplicate)
                    throw new InvalidOperationException("Ya existe un plato con ese nombre.");
            }

            if (dto.CategoryId.HasValue)
            {
                var catExists = await _db.Categories.AnyAsync(x => x.Id == dto.CategoryId.Value, ct);
                if (!catExists) throw new KeyNotFoundException("La categoría no existe.");
                dish.CategoryId = dto.CategoryId.Value;
            }

            // Asignaciones (parcial/total)
            if (!string.IsNullOrWhiteSpace(dto.Name)) dish.Name = dto.Name!;
            if (dto.Description is not null) dish.Description = dto.Description;
            if (dto.Price.HasValue) dish.Price = dto.Price.Value;
            if (dto.Image is not null) dish.ImageUrl  = dto.Image;
            if (dto.IsActive.HasValue) dish.Available = dto.IsActive.Value;

            dish.UpdateDate = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            return Map(dish);
        }
        private static DishResponseDto Map(Domain.Entities.Dish d) => new()
        {
            Id = d.DishId,
            Name = d.Name,
            Description = d.Description,
            Price = d.Price,
            Category = d.Category,     // si preferís un CategoryDto, avisame y lo cambiamos
            Image = d.ImageUrl,
            IsActive = d.Available,
            CreatedAt = d.CreateDate,
            UpdatedAt = d.UpdateDate
        };
        //public async Task<DishResponseDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        //{
        //    var dish = await _db.Dishes
        //        .Include(d => d.Category)
        //        .AsNoTracking()
        //        .FirstOrDefaultAsync(d => d.DishId == id, ct);

        //    if (dish is null) return null;

        //    return new DishResponseDto
        //    {
        //        Id = dish.DishId,
        //        Name = dish.Name,
        //        Description = dish.Description,
        //        Price = dish.Price,
        //        Category = new { id = dish.Category!.Id, name = dish.Category!.Name },
        //        Image = dish.ImageUrl,
        //        IsActive = dish.Available,
        //        CreatedAt = dish.CreateDate,
        //        UpdatedAt = dish.UpdateDate
        //    };
        //}
        public async Task<IEnumerable<DishResponseDto>> SearchAsync(DishFilterQuery q, CancellationToken ct)
        {
            var query = _db.Dishes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q.Name))
                query = query.Where(d => d.Name.Contains(q.Name));

            if (q.Category.HasValue)
                query = query.Where(d => d.CategoryId == q.Category.Value);

            if (q.OnlyActive.HasValue)
                query = q.OnlyActive.Value ? query.Where(d => d.Available) : query;

            if (q.SortByPrice.HasValue)
                query = q.SortByPrice == PriceSort.asc
                    ? query.OrderBy(d => d.Price)
                    : query.OrderByDescending(d => d.Price);

            return await query
                .Select(d => new DishResponseDto
                {
                    Id = d.DishId,
                    Name = d.Name,
                    Description = d.Description,
                    Price = d.Price,
                    Category = d.Category
                })
                .ToListAsync(ct);
        }
    }
}