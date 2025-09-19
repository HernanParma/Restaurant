using System.Threading;
using System.Threading.Tasks;
using Application.Dtos;

namespace Application.Interfaces
{
    public interface IUpdateOrderItemStatusService
    {
        Task<OrderUpdatedResponseDto> UpdateItemStatusAsync(long orderId, long itemId, OrderItemStatusUpdateDto dto, CancellationToken ct = default);
    }
}
