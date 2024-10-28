namespace Backend.Domain.DTOs;

public class ProfileDto
{
    public Guid UserId { get; set; }
    public List<SkillDto> Skills { get; set; }
    public List<LanguageDto> Languages { get; set; }
    public List<ProjectDto> Projects { get; set; }
    public List<CompanyDto> Companies { get; set; }
}
