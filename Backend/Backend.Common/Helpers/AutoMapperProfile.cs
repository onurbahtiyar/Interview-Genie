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
            CreateMap<Project, ProjectDto>().ReverseMap();
            CreateMap<Company, CompanyDto>().ReverseMap();
            CreateMap<Project, ProjectDto>()
                .ForMember(dest => dest.Skills, opt => opt.MapFrom(src => src.ProjectSkills.Select(ps => ps.Skill)));
            CreateMap<ProjectDto, Project>()
                .ForMember(dest => dest.ProjectSkills, opt => opt.Ignore());
        }
    }
}
