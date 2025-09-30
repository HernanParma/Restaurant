using Application.Dtos;
using Application.Interfaces;
using Application.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [ApiExplorerSettings(GroupName = "Dish")]
    [Produces("application/json")]
    public class DishController : ControllerBase
    {
        private readonly ICreateDishService _createService;
        private readonly IUpdateDishService _updateService;
        private readonly IGetAllDishesService _readService;
        private readonly IDishQuery _query;
        private readonly IDeleteDishService _deleteService;
        public DishController(
            ICreateDishService createService,
            IUpdateDishService updateService,
            IGetAllDishesService readService,
            IDishQuery query,
            IDeleteDishService deleteService)
        {
            _createService = createService;
            _updateService = updateService;
            _readService = readService;
            _query = query;
            _deleteService = deleteService;
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Crear nuevo plato")]
        [ProducesResponseType(typeof(DishResponseDto), StatusCodes.Status201Created)]
        public async Task<ActionResult<DishResponseDto>> Create([FromBody] DishCreateRequestDto req, CancellationToken ct)
        {
            var dto = new DishCreateDto
            {
                Name = req.Name,
                Description = req.Description,
                Price = req.Price,
                Category = req.Category,
                Image = req.Image,
                IsActive = true
            };

            var created = await _createService.CreateAsync(dto, ct);
            return CreatedAtRoute("GetDishById", new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        [SwaggerOperation(Summary = "Actualizar plato existente")]
        [ProducesResponseType(typeof(DishResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<DishResponseDto>> Update(Guid id, [FromBody] DishUpdateDto dto, CancellationToken ct)
        {
            var updated = await _updateService.UpdateAsync(id, dto, ct);
            return Ok(updated);
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Buscar platos")]
        public async Task<ActionResult<IEnumerable<DishResponseDto>>> Get([FromQuery] DishFilterQuery filter, CancellationToken ct)
        {
            var results = await _readService.SearchOrAllAsync(filter, ct);
            return Ok(results);
        }

        [HttpGet("{id:guid}", Name = "GetDishById")]
        [SwaggerOperation(Summary = "Obtener plato por ID")]

        public async Task<ActionResult<DishResponseDto>> GetById(Guid id, CancellationToken ct)
        {
            var dish = await _query.GetByIdAsync(id, ct);
            return dish is null ? NotFound() : Ok(dish);
        }
        [HttpDelete("{id:guid}")]
        [SwaggerOperation(Summary = "Eliminar plato",
        Description = "Elimina un plato. Solo se eliminan platos que no estén en órdenes activas. Se recomienda desactivar en lugar de eliminar.")]
        [ProducesResponseType(typeof(DishResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<DishResponseDto>> Delete(Guid id, CancellationToken ct)
        {
            var deleted = await _deleteService.DeleteAsync(id, ct);
            return Ok(deleted);
        }   
    }
}
