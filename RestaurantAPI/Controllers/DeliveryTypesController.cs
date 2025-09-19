using Application.Dtos;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace RestaurantAPI.Controllers;

[ApiController]
[Route("api/v1/deliverytypes")]
[ApiExplorerSettings(GroupName = "Dish")]
public class DeliveryTypesController : ControllerBase
{
    private readonly IDeliveryTypeQuery _query;
    public DeliveryTypesController(IDeliveryTypeQuery query) => _query = query;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DeliveryTypeDto>>> GetAll(CancellationToken ct) =>
        Ok(await _query.GetAllAsync(ct));
}
