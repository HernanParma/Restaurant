using Application.Dtos;

namespace Application.Interfaces
{
    public interface ICreateDishService
    {
        Task<DishResponseDto> CreateAsync(DishCreateDto dto, CancellationToken ct = default);
    }
}
