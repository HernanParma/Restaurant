using Application.Dtos;
using Application.Queries;

namespace Application.Interfaces
{
    public interface IDishQuery
    {
        Task<IEnumerable<DishResponseDto>> GetAllAsync(CancellationToken ct = default);
        Task<IEnumerable<DishResponseDto>> SearchAsync(DishFilterQuery query, CancellationToken ct = default);
        Task<DishResponseDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

    }
}