namespace Backend.Domain.DTOs;

public class CompanyDto
{
    public Guid? Id { get; set; }
    public string CompanyName { get; set; }
    public string Position { get; set; }
    public string Description { get; set; }
}
