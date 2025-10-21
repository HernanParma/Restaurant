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

        private const int Closed = 5;

        public UpdateOrderService(IOrderQuery orderQuery, IDishQuery dishQuery, IOrderCommand orderCommand)
        {
            _orderQuery = orderQuery;
            _dishQuery = dishQuery;
            _orderCommand = orderCommand;
        }

        public async Task<OrderUpdatedResponseDto> PatchAsync(long orderId, OrderPatchDto dto, CancellationToken ct = default)
        {
            var (exists, statusId) = await _orderQuery.GetExistsAndStatusAsync(orderId, ct);
            if (!exists) throw new NotFoundException("Orden no encontrada");

            if (statusId == Closed)
                throw new BusinessRuleException("No se puede modificar una orden cerrada");

            if (dto.Items != null && dto.Items.Count == 0)
                throw new BusinessRuleException("Si se envían 'items', debe haber al menos una operación.");

            if (dto.Items != null)
            {
                foreach (var op in dto.Items)
                {
                    if (string.IsNullOrWhiteSpace(op.Op))
                        throw new BusinessRuleException("Falta 'op' en un item.");

                    var kind = op.Op.Trim().ToLowerInvariant();

                    if (kind is "add" or "update")
                    {
                        if (op.DishId is null && op.OrderItemId is null)
                            throw new BusinessRuleException("Para add/update se requiere DishId u OrderItemId.");
                        if (op.Quantity is int q && q <= 0)
                            throw new BusinessRuleException("Las cantidades deben ser mayores a 0.");
                    }
                    else if (kind is not "remove")
                    {
                        throw new BusinessRuleException($"Operación inválida: {op.Op}");
                    }
                }
            }

            return await _orderCommand.PatchAsync(orderId, dto, ct);
        }
    }
}
