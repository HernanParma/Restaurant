using System.Text.RegularExpressions;
using Application.Constants;
using Application.Dtos;
using Application.Exceptions;
using Application.Interfaces;

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

            var deliveryType = await _deliveryQuery.GetByIdAsync(dto.Delivery.Id, ct);
            if (deliveryType is null)
                throw new NotFoundException("El tipo de entrega especificado no existe.");

            var to = dto.Delivery.To.Trim();

            
            static bool IsTable(string s) =>
                Regex.IsMatch(s, @"^Mesa\s+\d+$", RegexOptions.IgnoreCase);

            static bool LooksLikeAddress(string s) =>
                Regex.IsMatch(s, @"\d");

            static bool LooksLikePersonName(string s) =>
                Regex.IsMatch(s, @"^\p{L}[\p{L}\s'’-]*$");

            switch (deliveryType.Id)
            {
                case DeliveryTypeIds.DineIn:   
                    if (!IsTable(to))
                        throw new BusinessRuleException("Para 'Dine in' el destino debe ser 'Mesa <número>'.");
                    break;

                case DeliveryTypeIds.Delivery: 
                    if (!LooksLikeAddress(to))
                        throw new BusinessRuleException("Para 'Delivery' el destino debe parecer una dirección.");
                    break;

                case DeliveryTypeIds.TakeAway: 
                    if (!LooksLikePersonName(to))
                        throw new BusinessRuleException("Para 'Take away' el destino debe ser un nombre de persona.");
                    break;

                default:
                    throw new BusinessRuleException("Tipo de entrega no soportado.");
            }

            var ids = dto.Items.Select(i => i.Id).ToArray();
            var dishes = await _dishQuery.GetBasicByIdsAsync(ids, ct);

            if (dishes.Count != ids.Length)
                throw new BusinessRuleException("El plato especificado no existe o no está disponible.");

            if (dishes.Any(d => !d.IsActive))
                throw new BusinessRuleException("El plato especificado no existe o no está disponible.");
            var model = new CreateOrderCommandModel
            {
                DeliveryTypeId = deliveryType.Id, 
                DeliveryTo = to,
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
            model.InitialStatusId = 1; 

            return await _orderCommand.CreateAsync(model, ct);
        }
    }
}
