using MainServer.DTOs.Common;
using MainServer.DTOs.Payments;
using MainServer.Services.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MainServer.Controllers;

[Route("api/payments")]
[Authorize]
public class PaymentsController(IPaymentService paymentService) : ApiControllerBase
{
    [HttpGet("{id:int}")]
    [ProducesResponseType<ApiResponse<PaymentResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPayment(int id, CancellationToken cancellationToken)
    {
        var result = await paymentService.GetPaymentByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("order/{orderId:int}")]
    [ProducesResponseType<ApiResponse<PaginationResponse<PaymentResponse>>>(
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentsByOrder(
        int orderId,
        [FromQuery] PaginationRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await paymentService.GetPaymentsByOrderIdAsync(
            orderId,
            request,
            cancellationToken
        );
        return Ok(result);
    }
}
