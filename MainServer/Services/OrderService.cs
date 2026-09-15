using MainServer.Data;
using MainServer.DTOs.Common;
using MainServer.DTOs.Orders;
using MainServer.Entities;
using MainServer.Entities.Enums;
using MainServer.Exceptions;
using MainServer.Helpers;
using MainServer.Mappings;
using Microsoft.EntityFrameworkCore;

namespace MainServer.Services;

public class OrderService(
    AppDbContext context,
    ICurrentUserService currentUser,
    ILogger<OrderService> logger)
{
    public async Task<ApiResponse<OrderResponse>> CreateOrderAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId;
        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var products = await context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        if (products.Count != productIds.Count)
        {
            throw new NotFoundException("One or more products were not found.");
        }

        var productMap = products.ToDictionary(p => p.Id);
        var orderItems = new List<OrderItem>();
        decimal totalAmount = 0;

        foreach (var item in request.Items)
        {
            if (!productMap.TryGetValue(item.ProductId, out var product))
            {
                throw new NotFoundException($"Product with id {item.ProductId} was not found.");
            }

            if (product.Stock < item.Quantity)
            {
                throw new BusinessRuleException(
                    $"Insufficient stock for product '{product.Name}'. Available: {product.Stock}, requested: {item.Quantity}.");
            }

            product.Stock -= item.Quantity;
            product.UpdatedAt = DateTime.UtcNow;

            var lineTotal = product.Price * item.Quantity;
            totalAmount += lineTotal;

            orderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = item.Quantity
            });
        }

        var now = DateTime.UtcNow;
        var order = new Order
        {
            UserId = userId,
            Status = OrderStatus.Pending,
            TotalAmount = totalAmount,
            CreatedAt = now,
            UpdatedAt = now,
            Items = orderItems
        };

        context.Orders.Add(order);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var createdOrder = await GetOrderEntityAsync(order.Id, cancellationToken);

        logger.LogInformation("Order {OrderId} created for user {UserId}", order.Id, userId);

        return new ApiResponse<OrderResponse>(true, createdOrder.ToResponse());
    }

    public async Task<ApiResponse<OrderResponse>> GetOrderByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await GetOrderEntityAsync(id, cancellationToken);

        if (!currentUser.IsAdmin && order.UserId != currentUser.UserId)
        {
            throw new ForbiddenException("You do not have access to this order.");
        }

        return new ApiResponse<OrderResponse>(true, order.ToResponse());
    }

    public async Task<ApiResponse<PaginationResponse<OrderResponse>>> GetMyOrdersAsync(
        PaginationRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = BuildOrderQuery()
            .Where(o => o.UserId == currentUser.UserId)
            .OrderByDescending(o => o.CreatedAt);

        var paged = await ProjectOrdersAsync(query, request, cancellationToken);
        return new ApiResponse<PaginationResponse<OrderResponse>>(true, paged);
    }

    public async Task<ApiResponse<PaginationResponse<OrderResponse>>> GetAdminOrdersAsync(
        OrderQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = BuildOrderQuery();

        if (request.Status.HasValue)
        {
            query = query.Where(o => o.Status == request.Status.Value);
        }

        if (request.UserId.HasValue)
        {
            query = query.Where(o => o.UserId == request.UserId.Value);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= request.FromDate.Value.ToUniversalTime());
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt <= request.ToDate.Value.ToUniversalTime());
        }

        query = query.OrderByDescending(o => o.CreatedAt);

        var paged = await ProjectOrdersAsync(query, request, cancellationToken);
        return new ApiResponse<PaginationResponse<OrderResponse>>(true, paged);
    }

    public async Task<ApiResponse<OrderResponse>> CancelOrderAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var order = await context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order is null)
        {
            throw new NotFoundException("Order not found.");
        }

        if (!currentUser.IsAdmin && order.UserId != currentUser.UserId)
        {
            throw new ForbiddenException("You do not have access to cancel this order.");
        }

        if (order.Status != OrderStatus.Pending)
        {
            throw new BusinessRuleException("Only pending orders can be cancelled.");
        }

        var productIds = order.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        var productMap = products.ToDictionary(p => p.Id);

        foreach (var item in order.Items)
        {
            if (productMap.TryGetValue(item.ProductId, out var product))
            {
                product.Stock += item.Quantity;
                product.UpdatedAt = DateTime.UtcNow;
            }
        }

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var cancelledOrder = await GetOrderEntityAsync(order.Id, cancellationToken);

        logger.LogInformation("Order {OrderId} cancelled by user {UserId}", order.Id, currentUser.UserId);

        return new ApiResponse<OrderResponse>(true, cancelledOrder.ToResponse());
    }

    private IQueryable<Order> BuildOrderQuery() =>
        context.Orders.AsNoTracking();

    private async Task<PaginationResponse<OrderResponse>> ProjectOrdersAsync(
        IQueryable<Order> query,
        PaginationRequest request,
        CancellationToken cancellationToken)
    {
        var projectedQuery = query.Select(o => new OrderResponse(
            o.Id,
            o.UserId,
            o.User.Email,
            o.Status,
            o.TotalAmount,
            o.CreatedAt,
            o.UpdatedAt,
            o.Items.Select(i => new OrderItemResponse(
                i.Id,
                i.ProductId,
                i.ProductName,
                i.UnitPrice,
                i.Quantity,
                i.UnitPrice * i.Quantity)).ToList()));

        return await projectedQuery.ToPagedResultAsync(request, cancellationToken);
    }

    private async Task<Order> GetOrderEntityAsync(int id, CancellationToken cancellationToken)
    {
        var order = await context.Orders
            .AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order is null)
        {
            throw new NotFoundException("Order not found.");
        }

        return order;
    }
}
