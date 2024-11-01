namespace Backend.Domain.DTOs;

public class LearningTreeDto
{
    public string Topic { get; set; }
    public string DifficultyLevel { get; set; } // "Kolay", "Orta", "Zor"
    public string Importance { get; set; }      // "Düşük", "Orta", "Yüksek"
    public List<LearningTreeDto> SubTopics { get; set; } // Alt konular
}
