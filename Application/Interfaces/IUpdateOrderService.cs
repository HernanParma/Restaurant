using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Dtos;

namespace Application.Interfaces
{
    public interface IUpdateOrderService
    {
        Task<OrderUpdatedResponseDto> PatchAsync(long orderId, OrderPatchDto dto, CancellationToken ct = default);
    }
}