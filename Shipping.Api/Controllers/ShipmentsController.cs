using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShipCore.DTOs;
using ShipCore.Services;

namespace ShipCore.Controllers;

[ApiController]
[Route("api/shipments")]
[Authorize]
public class ShipmentsController : ControllerBase
{
    private readonly ShipmentService _service;

    public ShipmentsController(ShipmentService service)
    {
        _service = service;
    }


    private int GetUserId()
    {
        var value = User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        return int.Parse(value!);
    }


    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();

        var shipments =
            await _service.GetAllAsync(userId);

        return Ok(shipments);
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = GetUserId();

        var shipment =
            await _service.GetByIdAsync(id, userId);

        if (shipment is null)
        {
            return NotFound();
        }

        return Ok(shipment);
    }


    [HttpPost]
    public async Task<IActionResult> Create(
        CreateShipmentRequest request)
    {
        var userId = GetUserId();

        var shipment =
            await _service.CreateAsync(
                request,
                userId
            );

        return Ok(shipment);
    }


    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(
        int id,
        UpdateShipmentStatusRequest request)
    {
        var userId = GetUserId();

        var shipment =
            await _service.UpdateStatusAsync(
                id,
                request.Status,
                userId
            );

        if (shipment is null)
        {
            return NotFound();
        }

        return Ok(shipment);
    }


    [HttpGet("{id}/tracking")]
    public async Task<IActionResult> GetTracking(int id)
    {
        var userId = GetUserId();

        var events =
            await _service.GetTrackingAsync(
                id,
                userId
            );

        return Ok(events);
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();

        var deleted =
            await _service.DeleteAsync(id, userId);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}