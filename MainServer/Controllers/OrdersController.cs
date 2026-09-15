using MainServer.DTOs.Common;
using MainServer.DTOs.Orders;
using MainServer.Entities.Enums;
using MainServer.Services.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MainServer.Controllers;

[Route("api/orders")]
[Authorize]
public class OrdersController(IOrderService orderService) : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType<ApiResponse<OrderResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await orderService.CreateOrderAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<ApiResponse<OrderResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrder(int id, CancellationToken cancellationToken)
    {
        var result = await orderService.GetOrderByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("my-orders")]
    [ProducesResponseType<ApiResponse<PaginationResponse<OrderListResponse>>>(
        StatusCodes.Status200OK
    )]
    public async Task<IActionResult> GetMyOrders(
        [FromQuery] PaginationRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await orderService.GetMyOrdersAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("admin")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType<ApiResponse<PaginationResponse<OrderListResponse>>>(
        StatusCodes.Status200OK
    )]
    public async Task<IActionResult> GetOrders(
        [FromQuery] OrderQueryRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await orderService.GetAdminOrdersAsync(request, cancellationToken);
        return Ok(result);
    }
}
