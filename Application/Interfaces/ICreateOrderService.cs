using System.Threading;
using System.Threading.Tasks;
using Application.Dtos;

namespace Application.Interfaces
{
    public interface ICreateOrderService
    {
        Task<OrderCreatedResponseDto> CreateAsync(OrderCreateDto dto, CancellationToken ct = default);
    }
}
