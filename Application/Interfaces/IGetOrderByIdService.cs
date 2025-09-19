using System.Threading;
using System.Threading.Tasks;
using Application.Dtos;

namespace Application.Interfaces
{
    public interface IGetOrderByIdService
    {
        Task<OrderListDto> GetAsync(long orderId, CancellationToken ct = default);
    }
}
