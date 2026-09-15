using MainServer.DTOs.Categories;
using MainServer.DTOs.Common;
using MainServer.Entities.Enums;
using MainServer.Services.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MainServer.Controllers;

[Route("api/categories")]
public class CategoriesController(ICategoryService categoryService) : ApiControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<ApiResponse<PaginationResponse<CategoryResponse>>>(
        StatusCodes.Status200OK
    )]
    public async Task<IActionResult> GetCategories(
        [FromQuery] PaginationRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await categoryService.GetCategoriesAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType<ApiResponse<CategoryResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategory(int id, CancellationToken cancellationToken)
    {
        var result = await categoryService.GetCategoryByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType<ApiResponse<CategoryResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateCategory(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await categoryService.CreateCategoryAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType<ApiResponse<CategoryResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCategory(
        int id,
        [FromBody] UpdateCategoryRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await categoryService.UpdateCategoryAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DeleteCategory(int id, CancellationToken cancellationToken)
    {
        await categoryService.DeleteCategoryAsync(id, cancellationToken);
        return NoContent();
    }
}
