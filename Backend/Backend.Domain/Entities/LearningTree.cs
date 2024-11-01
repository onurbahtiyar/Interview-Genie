namespace Backend.Domain.Entities;

public class LearningTree
{
    public Guid Id { get; set; }
    public Guid InterviewSessionId { get; set; }
    public InterviewSession InterviewSession { get; set; }
    public string Topic { get; set; }
    public string DifficultyLevel { get; set; }
    public string Importance { get; set; }
}
