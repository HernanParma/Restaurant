using Application.Dtos;
using Application.Interfaces;
using Application.Queries;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace RestaurantAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class DishController : ControllerBase
    {
        private readonly ICreateDishService _createService;
        private readonly IUpdateDishService _updateService;
        private readonly IGetAllDishesService _readService;
        private readonly IDishQuery _query;

        public DishController(
            ICreateDishService createService,
            IUpdateDishService updateService,
            IGetAllDishesService readService,
            IDishQuery query)
        {
            _createService = createService;
            _updateService = updateService;
            _readService = readService;
            _query = query;
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Crear nuevo plato",
            Description = "Crea un plato. Reglas: nombre único, precio > 0 y categoría existente."
        )]
        public async Task<ActionResult<DishResponseDto>> Create([FromBody] DishCreateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            try
            {
                var created = await _createService.CreateAsync(dto, ct);
                return CreatedAtRoute("GetDishById", new { id = created.Id }, created);
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return BadRequest(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        }

        [HttpPut("{id:guid}")]
        [SwaggerOperation(
            Summary = "Actualizar plato existente",
            Description = "Actualiza los datos del plato indicado. Valida nombre único (solo si cambia), precio > 0 y categoría existente."
        )]
        public async Task<ActionResult<DishResponseDto>> Update(Guid id, [FromBody] DishUpdateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            try
            {
                var updated = await _updateService.UpdateAsync(id, dto, ct);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Buscar platos",
            Description = "Si no enviás filtros, devuelve todo. Podés filtrar por nombre, categoría, solo activos y ordenar por precio."
        )]
        public async Task<ActionResult<IEnumerable<DishResponseDto>>> Get([FromQuery] DishFilterQuery filter, CancellationToken ct)
        {
            bool hasFilters =
                !string.IsNullOrWhiteSpace(filter?.Name) ||
                filter?.Category.HasValue == true ||
                filter?.SortByPrice.HasValue == true ||
                filter?.OnlyActive.HasValue == true;

            var results = hasFilters
                ? await _readService.SearchAsync(filter!, ct)
                : await _readService.GetAllAsync(ct);

            return Ok(results);
        }

        //[HttpGet("{id:guid}", Name = "GetDishById")]
        //[SwaggerOperation(
        //    Summary = "Obtener plato por Id",
        //    Description = "Devuelve el plato con su categoría si existe."
        //)]
        //public async Task<ActionResult<DishResponseDto>> GetById(Guid id, CancellationToken ct)
        //{
        //    // Requiere que IDishQuery exponga GetByIdAsync
        //    var dish = await _query.GetByIdAsync(id, ct);
        //    return dish is null ? NotFound() : Ok(dish);
        //}
    }
}
