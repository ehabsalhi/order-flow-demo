using MainServer.DTOs.Common;
using MainServer.DTOs.Products;
using MainServer.Entities.Enums;
using MainServer.Services.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MainServer.Controllers;

[Route("api/products")]
public class ProductsController(IProductService productService) : ApiControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<ApiResponse<PaginationResponse<ProductResponse>>>(
        StatusCodes.Status200OK
    )]
    public async Task<IActionResult> GetProducts(
        [FromQuery] ProductQueryRequest query,
        CancellationToken cancellationToken
    )
    {
        var result = await productService.GetProductsAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType<ApiResponse<ProductResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProduct(int id, CancellationToken cancellationToken)
    {
        var result = await productService.GetProductByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType<ApiResponse<ProductResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateProduct(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await productService.CreateProductAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType<ApiResponse<ProductResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(
        int id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await productService.UpdateProductAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(int id, CancellationToken cancellationToken)
    {
        await productService.DeleteProductAsync(id, cancellationToken);
        return NoContent();
    }
}
