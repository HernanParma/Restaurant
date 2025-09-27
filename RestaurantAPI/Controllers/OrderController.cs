using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace RestaurantAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [ApiExplorerSettings(GroupName = "Order")]
    [Produces("application/json")]
    public class OrderController : ControllerBase
    {
        private readonly ICreateOrderService _create;
        private readonly IGetOrdersService _get;
        private readonly IUpdateOrderService _update;
        private readonly IGetOrderByIdService _getById;
        private readonly IUpdateOrderItemStatusService _updateItemStatus;

        public OrderController(
            ICreateOrderService create,
            IGetOrdersService get,
            IUpdateOrderService update,
            IGetOrderByIdService getById,
            IUpdateOrderItemStatusService updateItemStatus)
        {
            _create = create;
            _get = get;
            _update = update;
            _getById = getById;
            _updateItemStatus = updateItemStatus;
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Crear nueva orden",
            Description = "Valida platos activos, cantidades, calcula total y crea la orden con ítems."
        )]
        [ProducesResponseType(typeof(OrderCreatedResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<OrderCreatedResponseDto>> Create([FromBody] OrderCreateDto dto, CancellationToken ct)
        {
            var res = await _create.CreateAsync(dto, ct);
            return StatusCode(StatusCodes.Status201Created, res);
        }

        [HttpGet]
        [SwaggerOperation(
           Summary = "Buscar órdenes",
           Description = "Obtiene una lista de órdenes con filtros opcionales: rango de fechas y estado."
        )]
        [ProducesResponseType(typeof(IEnumerable<OrderListDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<OrderListDto>>> Search([FromQuery] OrderFilterQuery filter, CancellationToken ct)
        {
            var list = await _get.SearchAsync(filter, ct);
            return Ok(list);
        }

        [HttpPut("{id:long}")]
        [SwaggerOperation(
            Summary = "Actualizar orden existente",
            Description = "Reemplaza los items de la orden (solo órdenes no cerradas). Valida platos activos y recalcúla el total."
        )]
        [ProducesResponseType(typeof(OrderUpdatedResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderUpdatedResponseDto>> Update(long id, [FromBody] OrderUpdateDto dto, CancellationToken ct)
        {
            var res = await _update.UpdateAsync(id, dto, ct);
            return Ok(res);
        }

        [HttpGet("{id:long}", Name = "GetOrderById")]
        [SwaggerOperation(
            Summary = "Obtener orden por número",
            Description = "Devuelve los detalles completos de una orden (estado, entrega, items y platos)."
        )]
        [ProducesResponseType(typeof(OrderListDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderListDto>> GetById(long id, CancellationToken ct)
        {
            var order = await _getById.GetAsync(id, ct);
            return order is null ? NotFound() : Ok(order);
        }

        [HttpPatch("{id:long}/item/{itemId:long}")]
        [SwaggerOperation(
            Summary = "Actualizar estado de item individual",
            Description = "Actualiza el estado de un item de la orden y ajusta automáticamente el estado general."
        )]
        [ProducesResponseType(typeof(OrderUpdatedResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderUpdatedResponseDto>> UpdateItemStatus(long id, long itemId, [FromBody] OrderItemStatusUpdateDto dto, CancellationToken ct)
        {
            var res = await _updateItemStatus.UpdateItemStatusAsync(id, itemId, dto, ct);
            return Ok(res);
        }
    }
}
