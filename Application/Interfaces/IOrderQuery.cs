using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Dtos;

namespace Application.Queries
{
    public interface IOrderQuery
    {
        Task<IReadOnlyList<OrderListDto>> SearchAsync(OrderFilterQuery filter, CancellationToken ct = default);
        Task<(bool exists, int statusId)> GetExistsAndStatusAsync(long orderId, CancellationToken ct = default);
        Task<OrderListDto?> GetByIdAsync(long orderId, CancellationToken ct = default);
        Task<(bool orderExists, bool itemExists, int currentItemStatusId)> GetItemStatusAsync(long orderId, long itemId, CancellationToken ct = default);
        Task<IReadOnlyList<int>> GetOrderItemStatusIdsAsync(long orderId, CancellationToken ct = default);

    }
}
