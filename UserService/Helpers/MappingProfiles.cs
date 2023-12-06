using AutoMapper;
using UserService.Dtos;
using UserService.Models;

namespace UserService.Helpers;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<User, UserProfileDto>();
        CreateMap<Cart, CartDto>();
        CreateMap<CartItem, CartItemDto>();
        CreateMap<Order, OrderDto>();
    }
}