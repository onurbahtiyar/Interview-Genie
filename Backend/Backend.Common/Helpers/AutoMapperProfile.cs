using AutoMapper;
using Backend.Domain.DTOs;
using Backend.Domain.Entities;

namespace Backend.Common.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserDto>();
            CreateMap<RegisterDto, User>();
        }
    }
}
