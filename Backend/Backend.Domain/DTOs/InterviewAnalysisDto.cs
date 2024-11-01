namespace Backend.Domain.DTOs;

public class InterviewAnalysisDto
{
    public List<AreaOfImprovementDto> AreasOfImprovement { get; set; }
    public List<LearningTreeDto> LearningTree { get; set; }
    public string OverallFeedback { get; set; }
}
