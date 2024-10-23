namespace Backend.Domain.Entities;

public class InterviewSession
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CompanyInfoId { get; set; }
    public CompanyInfo CompanyInfo { get; set; }
    public List<InterviewQuestion> Questions { get; set; } = new List<InterviewQuestion>();
    public bool IsActive { get; set; } = true;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
}