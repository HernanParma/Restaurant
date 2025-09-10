using Application.Dtos;

namespace Application.Interfaces
{
    public interface IUpdateDishService
    {
        Task<DishResponseDto> UpdateAsync(Guid id, DishUpdateDto dto, CancellationToken ct = default);
    }
}
