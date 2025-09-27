using Application.Dtos;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace RestaurantAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[ApiExplorerSettings(GroupName = "Dish")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryQuery _query;
    public CategoryController(ICategoryQuery query) => _query = query;

    [HttpGet]
    [SwaggerOperation(Summary = "Obtener categorias de platos")]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetAll(CancellationToken ct) =>
        Ok(await _query.GetAllAsync(ct));
}
