using MainServer.DTOs.Common;
using MainServer.Entities.Enums;

namespace MainServer.DTOs.Orders;

public class OrderQueryRequest : PaginationRequest
{
    public OrderStatus? Status { get; set; }
    public int? UserId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
