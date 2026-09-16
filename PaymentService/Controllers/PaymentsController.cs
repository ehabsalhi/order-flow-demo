using Microsoft.AspNetCore.Mvc;
using PaymentService.DTOs;
using PaymentService.DTOs.Common;
using PaymentService.Services;

namespace PaymentService.Controllers;

[Route("api/payments")]
public class PaymentsController(IPaymentService paymentService) : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType<ApiResponse<PaymentResponse>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreatePayment(
        [FromBody] CreatePaymentRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await paymentService.CreatePaymentAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetPayment), new { id = result.Data!.PaymentId }, result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<ApiResponse<PaymentResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPayment(int id, CancellationToken cancellationToken)
    {
        var result = await paymentService.GetPaymentByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("order/{orderId:int}")]
    [ProducesResponseType<ApiResponse<PaginationResponse<PaymentResponse>>>(
        StatusCodes.Status200OK
    )]
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
