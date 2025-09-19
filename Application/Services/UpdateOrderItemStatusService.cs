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
        private const int InPreparation = 2;  
        private const int Ready = 3;
        private const int Delivered = 4;
        private const int Cancelled = 5;      

        public UpdateOrderItemStatusService(IOrderQuery orderQuery, IStatusQuery statusQuery, IOrderCommand orderCommand)
        {
            _orderQuery = orderQuery;
            _statusQuery = statusQuery;
            _orderCommand = orderCommand;
        }

        public async Task<OrderUpdatedResponseDto> UpdateItemStatusAsync(long orderId, long itemId, OrderItemStatusUpdateDto dto, CancellationToken ct = default)
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

            return from switch
            {
                var f when f == Pending => to is InPreparation or Cancelled,
                var f when f == InPreparation => to is Ready or Cancelled,
                var f when f == Ready => to is Delivered or Cancelled,
                var f when f == Delivered => false,
                var f when f == Cancelled => false,
                _ => false
            };
        }

        private static int ComputeOverallStatus(System.Collections.Generic.IReadOnlyList<int> itemStatuses)
        {
           
            if (itemStatuses.Count == 0) return Pending;

            bool anyInPrep = itemStatuses.Any(s => s == InPreparation);
            bool anyPending = itemStatuses.Any(s => s == Pending);
            bool anyReady = itemStatuses.Any(s => s == Ready);
            bool anyDelivered = itemStatuses.Any(s => s == Delivered);
            bool anyNonDelivered = itemStatuses.Any(s => s != Delivered);

            if (!anyNonDelivered) return Delivered; // todos delivered

            if (!anyPending && !anyInPrep && anyReady) return Ready;

            if (anyInPrep) return InPreparation;

            return Pending;
        }
    }
}
