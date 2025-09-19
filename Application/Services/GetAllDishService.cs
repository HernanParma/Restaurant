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
        public Task<IEnumerable<DishResponseDto>> SearchOrAllAsync(DishFilterQuery? query, CancellationToken ct = default)
        {
            var hasFilters = query is not null && (
                !string.IsNullOrWhiteSpace(query.Name) ||
                query.Category.HasValue ||
                query.SortByPrice.HasValue ||
                query.OnlyActive.HasValue
            );

            return hasFilters
                ? _query.SearchAsync(query!, ct)
                : _query.GetAllAsync(ct);
        }
    }
}