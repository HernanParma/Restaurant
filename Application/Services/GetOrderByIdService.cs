using System.Threading;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Exceptions; 
using Application.Interfaces;
using Application.Queries;

namespace Application.Services
{
    public sealed class GetOrderByIdService : IGetOrderByIdService
    {
        private readonly IOrderQuery _query;

        public GetOrderByIdService(IOrderQuery query) => _query = query;

        public async Task<OrderListDto> GetAsync(long orderId, CancellationToken ct = default)
        {
            var order = await _query.GetByIdAsync(orderId, ct);
            if (order is null) throw new NotFoundException("Orden no encontrada");
            return order;
        }
    }
}
