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
    public sealed class UpdateOrderService : IUpdateOrderService
    {
        private readonly IOrderQuery _orderQuery;
        private readonly IDishQuery _dishQuery;
        private readonly IOrderCommand _orderCommand;

        // definí tus estados “cerrada” (por ejemplo: 3=Completed, 4=Cancelled)
        private readonly int[] _closedStatuses = new[] { 3, 4, 5 };

        public UpdateOrderService(IOrderQuery orderQuery, IDishQuery dishQuery, IOrderCommand orderCommand)
        {
            _orderQuery = orderQuery;
            _dishQuery = dishQuery;
            _orderCommand = orderCommand;
        }

        public async Task<OrderUpdatedResponseDto> UpdateAsync(long orderId, OrderUpdateDto dto, CancellationToken ct = default)
        {
            if (dto.Items is null || dto.Items.Count == 0)
                throw new BusinessRuleException("Debe especificar al menos un ítem.");

            if (dto.Items.Any(i => i.Quantity <= 0))
                throw new BusinessRuleException("Las cantidades deben ser mayores a 0.");

            // 1) Debe existir y no estar cerrada
            var (exists, statusId) = await _orderQuery.GetExistsAndStatusAsync(orderId, ct);
            if (!exists) throw new NotFoundException("Orden no encontrada");
            if (_closedStatuses.Contains(statusId))
                throw new BusinessRuleException("No se puede modificar una orden que ya está cerrada");

            // 2) Validar platos activos
            var ids = dto.Items.Select(i => i.Id).ToArray();
            var dishes = await _dishQuery.GetBasicByIdsAsync(ids, ct);
            if (dishes.Count != ids.Length)
                throw new BusinessRuleException("Alguno de los platos no existe.");
            if (dishes.Any(d => !d.IsActive))
                throw new BusinessRuleException("No se pueden agregar items de platos inactivos.");

            // 3) Construir nuevos items a persistir y total
            var newItems = dto.Items.Select(i =>
            {
                var db = dishes.First(d => d.Id == i.Id);
                return new OrderItemToPersist
                {
                    DishId = i.Id,
                    Quantity = i.Quantity,
                    UnitPrice = db.Price,
                    Notes = i.Notes
                };
            }).ToList();

            var newTotal = newItems.Sum(x => x.UnitPrice * x.Quantity);

            // 4) Persistir (reemplazo full de items)
            return await _orderCommand.UpdateItemsAsync(orderId, newItems, newTotal, ct);
        }
    }
}
