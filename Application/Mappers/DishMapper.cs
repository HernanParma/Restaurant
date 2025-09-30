using Application.Dtos;
using Domain.Entities;
using System.Linq.Expressions;

namespace Application.Mappers
{
    public static class DishMapper
    {
        public static Expression<Func<Dish, DishResponseDto>> ToDtoProjection() => d => new DishResponseDto
        {
            Id = d.DishId,
            Name = d.Name,
            Description = d.Description ?? string.Empty,
            Price = d.Price,
            Available = d.Available,
            ImageUrl = d.ImageUrl ?? string.Empty,
            Category = new CategoryLiteDto
            {
                Id = d.CategoryId,
                Name = d.Category.Name
            },
            CreatedAt = d.CreateDate,
            UpdatedAt = d.UpdateDate
        };
    }
}
