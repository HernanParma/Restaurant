using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Dtos;

namespace Application.Interfaces
{
    public interface IUpdateOrderService
    {
        Task<OrderUpdatedResponseDto> UpdateAsync(long orderId, OrderUpdateDto dto, CancellationToken ct = default);
    }
}
