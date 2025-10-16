using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Exceptions;
using Application.Interfaces;
using Application.Queries;

namespace Application.Services
{
    public sealed class GetOrdersService : IGetOrdersService
    {
        private readonly IOrderQuery _query;
        public GetOrdersService(IOrderQuery query) => _query = query;

        public async Task<IReadOnlyList<OrderListDto>> SearchAsync(OrderFilterQuery filter, CancellationToken ct = default)
        {
            filter ??= new OrderFilterQuery();

            if (filter.From.HasValue && filter.To.HasValue && filter.From.Value > filter.To.Value)
                throw new BusinessRuleException("Rango de fechas inválido");

            filter.DeliveryTo = string.IsNullOrWhiteSpace(filter.DeliveryTo) ? null : filter.DeliveryTo.Trim();

            return await _query.SearchAsync(filter, ct);
        }
    }
}
