namespace Backend.Domain.DTOs;

public class GenerateTrainingPlanRequest
{
    public string Topic { get; set; }
    public string Language { get; set; } = "Türkçe";
}
