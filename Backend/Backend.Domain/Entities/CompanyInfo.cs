namespace Backend.Domain.Entities;

public class CompanyInfo
{
    public Guid Id { get; set; }
    public string CompanyName { get; set; }
    public string Industry { get; set; }
    public string Location { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
