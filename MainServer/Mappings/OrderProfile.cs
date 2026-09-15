using AutoMapper;
using MainServer.DTOs.Orders;
using MainServer.Entities;

namespace MainServer.Mappings;

public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<OrderItem, OrderItemResponse>()
            .ForCtorParam(
                nameof(OrderItemResponse.LineTotal),
                opt => opt.MapFrom(src => src.Price * src.Quantity));

        CreateMap<Order, OrderResponse>();
    }
}
