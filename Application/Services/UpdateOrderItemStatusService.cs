using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Exceptions; 
using Application.Interfaces;
using Application.Queries;

namespace Application.Services
{
    public sealed class UpdateOrderItemStatusService : IUpdateOrderItemStatusService
    {
        private readonly IOrderQuery _orderQuery;
        private readonly IStatusQuery _statusQuery;
        private readonly IOrderCommand _orderCommand;

        private const int Pending = 1;
        private const int InProgress = 2;
        private const int Ready = 3;
        private const int Delivery = 4;
        private const int Closed = 5;

        public UpdateOrderItemStatusService(IOrderQuery orderQuery, IStatusQuery statusQuery, IOrderCommand orderCommand)
        {
            _orderQuery = orderQuery;
            _statusQuery = statusQuery;
            _orderCommand = orderCommand;
        }

        public async Task<OrderUpdatedResponseDto> UpdateItemStatusAsync(
            long orderId, long itemId, OrderItemStatusUpdateDto dto, CancellationToken ct = default)
        {
            if (dto is null) throw new BusinessRuleException("Body requerido.");
            var newStatus = dto.Status;

            var (orderExists, itemExists, current) = await _orderQuery.GetItemStatusAsync(orderId, itemId, ct);
            if (!orderExists) throw new NotFoundException("Orden no encontrada");
            if (!itemExists) throw new NotFoundException("Item no encontrado");

            if (!await _statusQuery.ExistsAsync(newStatus, ct))
                throw new BusinessRuleException("El estado especificado no es válido");

            if (!IsAllowedTransition(current, newStatus))
                throw new BusinessRuleException("Transición de estado no permitida");

            var allItemStatuses = (await _orderQuery.GetOrderItemStatusIdsAsync(orderId, ct)).ToList();
            var idx = allItemStatuses.FindIndex(s => s == current);
            if (idx >= 0) allItemStatuses[idx] = newStatus;

            var newOverall = ComputeOverallStatus(allItemStatuses);

            return await _orderCommand.UpdateItemStatusAsync(orderId, itemId, newStatus, newOverall, ct);
        }

        private static bool IsAllowedTransition(int from, int to)
        {
            if (from == to) return true; 

            return (from, to) switch
            {
                (Pending, InProgress) => true,
                (InProgress, Ready) => true,
                (Ready, Delivery) => true,
                (Delivery, Closed) => true,
                _ => false
            };
        }

        private static int ComputeOverallStatus(IReadOnlyList<int> s)
        {
            if (s.Count == 0) return Pending;

            bool anyPending = s.Any(x => x == Pending);
            bool anyInProgress = s.Any(x => x == InProgress);
            bool anyReady = s.Any(x => x == Ready);
            bool anyDelivery = s.Any(x => x == Delivery);
            bool anyClosed = s.Any(x => x == Closed);

            if (s.All(x => x == Closed)) return Closed;

            if (anyDelivery && s.All(x => x == Ready || x == Delivery || x == Closed))
                return Delivery;

            if (!anyDelivery && !anyInProgress && !anyPending && anyReady)
                return Ready;

            if (anyInProgress) return InProgress;

            if (anyPending) return Pending;

            return Pending;
        }
    }

}
