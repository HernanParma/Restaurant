using Application.Dtos;
using Application.Queries;

namespace Application.Interfaces
{
    public interface IDishQuery
    {
        Task<IEnumerable<DishResponseDto>> GetAllAsync(CancellationToken ct = default);
        Task<IEnumerable<DishResponseDto>> SearchAsync(DishFilterQuery query, CancellationToken ct = default);
        Task<DishResponseDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
        Task<bool> IsInActiveOrdersAsync(Guid dishId, CancellationToken ct = default);
        Task<IReadOnlyList<DishBasicInfo>> GetBasicByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
        public class DishBasicInfo
        {
            public Guid Id { get; set; }
            public decimal Price { get; set; }
            public bool IsActive { get; set; }
        }
    }
}