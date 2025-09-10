// Application/Dishes/Mappers/DishMapper.cs
using Application.Dtos;
using Domain.Entities;
using System.Linq.Expressions;

namespace Application.Mappers
{
    public static class DishMapper
    {
        public static DishResponseDto ToDto(this Dish d) => new()
        {
            Id = d.DishId,
            Name = d.Name,
            Description = d.Description,
            Price = d.Price,
            IsActive = d.Available,
            Image = d.ImageUrl,
            Category = d.Category == null ? null
                        : new CategoryDto { Id = d.Category.Id, Name = d.Category.Name },
            CreatedAt = d.CreateDate,
            UpdatedAt = d.UpdateDate
        };
        public static Expression<Func<Dish, DishResponseDto>> ToDtoProjection() => d => new DishResponseDto
        {
            Id = d.DishId,
            Name = d.Name,
            Description = d.Description,
            Price = d.Price,
            IsActive = d.Available,
            Image = d.ImageUrl,
            Category = d.Category == null ? null
                        : new CategoryDto { Id = d.Category.Id, Name = d.Category.Name },
            CreatedAt = d.CreateDate,
            UpdatedAt = d.UpdateDate
        };
    }
}
