using Application.Dishes.Command.CreateDish;
using Application.Dishes.Command.CreateDish;
using Application.Dishes.Command.UpdateDish;
using Application.Dishes.Dtos;
using Application.Queries;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using static Application.Dishes.Command.UpdateDish.DishUpdateCommand;

namespace RestaurantAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class DishController : ControllerBase
    {
        private readonly ICreateDishHandler _create;
        private readonly IDishUpdateHandler _update;
        private readonly IDishQuery _query;

        public DishController(
            ICreateDishHandler create,
            IDishUpdateHandler update,
            IDishQuery query)
        {
            _create = create;
            _update = update;
            _query = query;
        }

        // POST: Commands
        [HttpPost]
        [SwaggerOperation(
            Summary = "Crear nuevo plato",
            Description = "Crea un plato. Reglas: nombre único, precio > 0 y categoría existente."
        )]
        [ProducesResponseType(typeof(DishResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<DishResponseDto>> Create([FromBody] DishCreateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            try
            {
                var created = await _create.HandleAsync(new CreateDishCommand(dto), ct);
                return CreatedAtRoute("GetDishById", new { id = created.Id }, created);
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return BadRequest(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        }

        // PUT: Commands
        [HttpPut("{id:guid}")]
        [SwaggerOperation(
            Summary = "Actualizar plato existente",
            Description = "Actualiza los datos del plato indicado. Mantiene las mismas reglas de validación."
        )]
        [ProducesResponseType(typeof(DishResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<DishResponseDto>> Update(Guid id, [FromBody] DishUpdateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            try
            {
                var updated = await _update.HandleAsync(new UpdateDishCommand(id, dto), ct);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        }

        // GET (list): Queries
        [HttpGet]
        [SwaggerOperation(
            Summary = "Buscar platos",
            Description = "Obtiene una lista de platos del menú con opciones de filtrado y ordenamiento."
        )]
        [ProducesResponseType(typeof(IEnumerable<DishResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<DishResponseDto>>> Get([FromQuery] DishFilterQuery filter, CancellationToken ct)
        {
            var results = await _query.SearchAsync(filter, ct);
            return Ok(results);
        }

        // GET (by id): Queries — oculto en Swagger pero ENTRA en routing
        [HttpGet("{id:guid}", Name = "GetDishById")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<DishResponseDto>> GetById(Guid id, CancellationToken ct)
        {
            var dish = await _query.GetByIdAsync(id, ct);
            return dish is null ? NotFound() : Ok(dish);
        }
    }
}
