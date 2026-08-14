using Microsoft.AspNetCore.Mvc;
using ShipCore.CarrierIntegrations;

namespace ShipCore.Controllers;

[ApiController]
[Route("api/carriers/{carrierCode}/shipments")]
public sealed class CarrierShipmentsController(ICarrierIntegrationResolver carriers) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ShipmentCreationResult>> Create(
        string carrierCode,
        CarrierShipmentRequest request,
        CancellationToken cancellationToken)
    {
        try { return Ok(await carriers.GetRequired(carrierCode).CreateShipmentAsync(request, cancellationToken)); }
        catch (KeyNotFoundException) { return NotFound(new { message = "Unsupported carrier." }); }
    }

    [HttpGet("{trackingNumber}/label")]
    public async Task<ActionResult<LabelResult>> GetLabel(
        string carrierCode,
        string trackingNumber,
        CancellationToken cancellationToken)
    {
        try { return Ok(await carriers.GetRequired(carrierCode).GetLabelAsync(trackingNumber, cancellationToken)); }
        catch (CarrierApiException exception) when (exception.StatusCode == 404)
        {
            return Accepted(new { status = "pending", trackingNumber });
        }
        catch (KeyNotFoundException) { return NotFound(new { message = "Unsupported carrier." }); }
    }
}
