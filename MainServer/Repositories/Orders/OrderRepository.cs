using MainServer.Data;
using MainServer.DTOs.Common;
using MainServer.DTOs.Orders;
using MainServer.Entities;
using MainServer.Entities.Enums;
using MainServer.Exceptions;
using MainServer.Helpers;
using Microsoft.EntityFrameworkCore;

namespace MainServer.Repositories.Orders;

public class OrderRepository(AppDbContext context) : IOrderRepository
{
    public async Task<Order> CreateAsync(
        int userId,
        IReadOnlyList<CreateOrderItemRequest> items,
        CancellationToken cancellationToken = default
    )
    {
        var productIds = items.Select(i => i.ProductId).Distinct().ToList();

        await using var transaction = await context.Database.BeginTransactionAsync(
            cancellationToken
        );

        var products = await context
            .Products.Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        if (products.Count != productIds.Count)
        {
            throw new NotFoundException("One or more products were not found.");
        }

        var productMap = products.ToDictionary(p => p.Id);
        var orderItems = new List<OrderItem>();
        decimal totalAmount = 0;

        foreach (var item in items)
        {
            if (!productMap.TryGetValue(item.ProductId, out var product))
            {
                throw new NotFoundException($"Product with id {item.ProductId} was not found.");
            }

            if (product.Stock < item.Quantity)
            {
                throw new BusinessRuleException(
                    $"Insufficient stock for product '{product.Name}'. Available: {product.Stock}, requested: {item.Quantity}."
                );
            }

            product.Stock -= item.Quantity;
            product.UpdatedAt = DateTime.UtcNow;
            totalAmount += product.Price * item.Quantity;

            orderItems.Add(
                new OrderItem
                {
                    ProductId = product.Id,
                    Price = product.Price,
                    Quantity = item.Quantity,
                }
            );
        }

        var now = DateTime.UtcNow;
        var order = new Order
        {
            UserId = userId,
            Status = OrderStatus.Pending,
            TotalAmount = totalAmount,
            CreatedAt = now,
            UpdatedAt = now,
            Items = orderItems,
        };

        context.Orders.Add(order);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return (await GetByIdAsync(order.Id, cancellationToken))!;
    }

    public Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        context
            .Orders.AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<PaginationResponse<OrderListResponse>> GetPagedByUserIdAsync(
        int userId,
        PaginationRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var query = context.Orders.AsNoTracking().Where(o => o.UserId == userId);

        return await ProjectPagedAsync(query, request, cancellationToken);
    }

    public async Task<PaginationResponse<OrderListResponse>> GetPagedAsync(
        OrderQueryRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var query = context.Orders.AsNoTracking();

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

        return await ProjectPagedAsync(query, request, cancellationToken);
    }

    private static async Task<PaginationResponse<OrderListResponse>> ProjectPagedAsync(
        IQueryable<Order> query,
        PaginationRequest request,
        CancellationToken cancellationToken
    )
    {
        var projected = query
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderListResponse(
                o.Id,
                o.UserId,
                o.Status,
                o.TotalAmount,
                o.CreatedAt,
                o.UpdatedAt,
                o.Items.Count()
            ));

        return await projected.ToPagedResultAsync(request, cancellationToken);
    }
}
