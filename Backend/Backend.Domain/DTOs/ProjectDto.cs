namespace Backend.Domain.DTOs;

public class ProjectDto
{
    public Guid? Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public List<SkillDto> Skills { get; set; }
}
