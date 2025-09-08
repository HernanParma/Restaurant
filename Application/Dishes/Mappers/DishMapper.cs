//using Application.Dishes.Dtos;
//using Domain.Entities;

//namespace Application.Dishes.Mappers
//{
//    public static class DishMapper
//    {
//        public static DishResponseDto ToResponseDto(this Dish d)
//        {
//            return new DishResponseDto
//            {
//                Id = d.DishId,
//                Name = d.Name,
//                Description = d.Description,
//                Price = d.Price,
//                Category = d.Category is null
//                    ? null
//                    : new CategoryDto { Id = d.Category.Id, Name = d.Category.Name },
//                Image = d.ImageUrl,
//                IsActive = d.Available,
//                CreatedAt = d.CreateDate,
//                UpdatedAt = d.UpdateDate
//            };
//        }
//    }
//}