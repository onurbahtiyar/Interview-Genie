namespace Backend.Domain.DTOs;

public class TrainingPlan
{
    public string Topic { get; set; }
    public List<TrainingStep> Steps { get; set; }
}
