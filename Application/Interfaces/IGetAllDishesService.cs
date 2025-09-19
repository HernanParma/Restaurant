using Application.Dtos;
using Application.Queries;

namespace Application.Interfaces
{
    public interface IGetAllDishesService
    {
        Task<IEnumerable<DishResponseDto>> GetAllAsync(CancellationToken ct = default);
        Task<IEnumerable<DishResponseDto>> SearchAsync(DishFilterQuery query, CancellationToken ct = default);
        Task<IEnumerable<DishResponseDto>> SearchOrAllAsync(DishFilterQuery? query, CancellationToken ct = default);

    }
}