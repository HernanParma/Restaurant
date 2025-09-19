using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Dtos;

namespace Application.Interfaces
{
    public interface IGetOrdersService
    {
        Task<IReadOnlyList<OrderListDto>> SearchAsync(OrderFilterQuery filter, CancellationToken ct = default);
    }
}
