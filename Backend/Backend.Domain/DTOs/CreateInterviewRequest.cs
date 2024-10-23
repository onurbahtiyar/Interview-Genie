namespace Backend.Domain.DTOs;

public class CreateInterviewRequest
{
    public string CompanyName { get; set; }
    public string Industry { get; set; }
    public string Location { get; set; }
    public List<string> Skills { get; set; }
    public string Description { get; set; }
    public string Language { get; set; }
}
