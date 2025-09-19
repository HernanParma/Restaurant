using Application.Dtos;

namespace Application.Interfaces
{
    public interface IDishCommand
    {
        Task<DishResponseDto> CreateAsync(DishCreateDto dto, CancellationToken ct = default);
        Task<DishResponseDto> UpdateAsync(Guid id, DishUpdateDto dto, CancellationToken ct = default);
        Task<DishResponseDto> DeleteAsync(Guid id, CancellationToken ct = default);

    }
}