using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShipCore.DTOs;
using ShipCore.Services;

namespace ShipCore.Controllers;

[ApiController]
[Route("api/carriers")]
[Authorize]
public class CarriersController : ControllerBase
{
    private readonly CarrierService _service;

    public CarriersController(CarrierService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var carriers = await _service.GetAllAsync();

        return Ok(carriers);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateCarrierRequest request)
    {
        var carrier =
            await _service.CreateAsync(request.Name);

        return Ok(carrier);
    }
}