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
    public sealed class CreateOrderService : ICreateOrderService
    {
        private readonly IDishQuery _dishQuery;
        private readonly IDeliveryTypeQuery _deliveryQuery;
        private readonly IOrderCommand _orderCommand;

        public CreateOrderService(IDishQuery dishQuery, IDeliveryTypeQuery deliveryQuery, IOrderCommand orderCommand)
        {
            _dishQuery = dishQuery;
            _deliveryQuery = deliveryQuery;
            _orderCommand = orderCommand;
        }

        public async Task<OrderCreatedResponseDto> CreateAsync(OrderCreateDto dto, CancellationToken ct = default)
        {
            if (dto.Items == null || dto.Items.Count == 0)
                throw new BusinessRuleException("Debe especificar al menos un ítem.");

            if (dto.Items.Any(i => i.Quantity <= 0))
                throw new BusinessRuleException("Las cantidades deben ser mayores a 0.");

            if (dto.Delivery == null || dto.Delivery.Id <= 0 || string.IsNullOrWhiteSpace(dto.Delivery.To))
                throw new BusinessRuleException("Debe especificar tipo de entrega y destino.");

            if (!await _deliveryQuery.ExistsAsync(dto.Delivery.Id, ct))
                throw new NotFoundException("El tipo de entrega especificado no existe.");

            var ids = dto.Items.Select(i => i.Id).ToArray();
            var dishes = await _dishQuery.GetBasicByIdsAsync(ids, ct);

            if (dishes.Count != ids.Length)
                throw new BusinessRuleException("El plato especificado no existe o no está disponible");

            if (dishes.Any(d => !d.IsActive))
                throw new BusinessRuleException("El plato especificado no existe o no está disponible.");

            // Construir modelo para el Command
            var model = new CreateOrderCommandModel
            {
                DeliveryTypeId = dto.Delivery.Id,
                DeliveryTo = dto.Delivery.To,
                Notes = dto.Notes,
                Items = dto.Items.Select(i =>
                {
                    var db = dishes.First(d => d.Id == i.Id);
                    return new OrderItemToPersist
                    {
                        DishId = i.Id,
                        Quantity = i.Quantity,
                        UnitPrice = db.Price,
                        Notes = i.Notes
                    };
                }).ToList()
            };

            model.TotalAmount = model.Items.Sum(x => x.UnitPrice * x.Quantity);
            model.InitialStatusId = 1; // Pending (ajustar a tus seeds)

            return await _orderCommand.CreateAsync(model, ct);
        }
    }
}
