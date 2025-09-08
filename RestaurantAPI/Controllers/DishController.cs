using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Application.Dishes;
using Application.Dishes.Dtos;
using Swashbuckle.AspNetCore.Annotations;
using Application.Queries;
using Infrastructure.Queries;

namespace RestaurantAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class DishController : ControllerBase
    {
        private readonly IDishService _service;
        private readonly IDishQuery _dishQuery;
        public DishController(IDishService service, IDishQuery dishQuery)
        {
            _service = service;
            _dishQuery = dishQuery;
        }
       
        
        [HttpPost]
        [SwaggerOperation(
            Summary = "Crear nuevo plato",
            Description = "Crea un plato. Reglas: nombre único, precio > 0 y categoría existente."
        )]
        //[SwaggerResponse(StatusCodes.Status201Created, "Plato creado exitosamente", typeof(DishResponseDto))]
        //[SwaggerResponse(StatusCodes.Status400BadRequest, "Datos de entrada inválidos")]
        //[SwaggerResponse(StatusCodes.Status409Conflict, "Ya existe un plato con el mismo nombre")]
        public async Task<IActionResult> Create([FromBody] DishCreateDto dto, CancellationToken ct)
        {
            try
            {
                var created = await _service.CreateAsync(dto, ct);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return BadRequest(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        }
       
        
        [HttpGet]
        [SwaggerOperation(
        Summary = "Buscar platos",
        Description =
        @"Obtiene una lista de platos del menú con opciones de filtrado y ordenamiento."
        )]
        //[ProducesResponseType(typeof(IEnumerable<DishResponseDto>), StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Get([FromQuery] DishFilterQuery query, CancellationToken ct)
        {
            var results = await _service.SearchAsync(query, ct);
            return Ok(results);
        }
       
        
        [HttpPut("{id:guid}")]
        [SwaggerOperation(
            Summary = "Actualizar plato existente",
            Description = "Actualiza los datos del plato indicado. Mantiene las mismas reglas de validación."
        )]
        public async Task<IActionResult> Update(Guid id, [FromBody] DishUpdateDto dto, CancellationToken ct)
        {
            try
            {
                var updated = await _service.UpdateAsync(id, dto, ct);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        }
       
        
        [HttpGet("{id:guid}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var dish = await _dishQuery.GetByIdAsync(id, ct);
            return dish is null ? NotFound() : Ok(dish);
        }
    }
}