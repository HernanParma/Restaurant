using Application.Dtos;
using Application.Interfaces;
using Application.Queries;

namespace Application.Services
{
    public class GetAllDishesService : IGetAllDishesService
    {
        private readonly IDishQuery _query;
        public GetAllDishesService(IDishQuery query) => _query = query;

        public Task<IEnumerable<DishResponseDto>> GetAllAsync(CancellationToken ct = default)
            => _query.GetAllAsync(ct);

        public Task<IEnumerable<DishResponseDto>> SearchAsync(DishFilterQuery filter, CancellationToken ct = default)
            => _query.SearchAsync(filter, ct);
    }
}