using AutoMapper;
using ProductService.Dtos;
using ProductService.Models;

namespace ProductService.Helpers;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Product, ProductDto>();
        CreateMap<Category, CategoryDto>(); // mapping without subcategories
    }
}