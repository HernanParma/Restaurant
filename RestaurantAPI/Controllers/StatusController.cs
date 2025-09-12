using Application.Dtos;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace RestaurantAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class StatusController : ControllerBase
{
    private readonly IStatusQuery _query;
    public StatusController(IStatusQuery query) => _query = query;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StatusDto>>> GetAll(CancellationToken ct) =>
        Ok(await _query.GetAllAsync(ct));
}
