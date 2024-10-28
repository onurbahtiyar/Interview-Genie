using Backend.Common.Results;
using Backend.Domain.DTOs;

namespace Backend.Application.Interfaces;

public interface IProfileService
{
    Task<IDataResult<ProfileDto>> GetProfileAsync(Guid userId);
    Task<IResult> UpdateSkillsAsync(Guid userId, List<string> skills);
    Task<IResult> UpdateLanguagesAsync(Guid userId, List<string> languages);
    Task<IResult> AddOrUpdateProjectsAsync(Guid userId, List<ProjectDto> projects);
    Task<IResult> AddOrUpdateCompaniesAsync(Guid userId, List<CompanyDto> companies);
    Task<IDataResult<List<SkillDto>>> GetAllSkillsAsync();
    Task<IDataResult<List<LanguageDto>>> GetAllLanguagesAsync();
}
