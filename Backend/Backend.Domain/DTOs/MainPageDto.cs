namespace Backend.Domain.DTOs;

public class MainPageDto
{
    public List<InterviewSessionDto> PastSessions { get; set; }
    public InterviewSummaryDto Summary { get; set; }
}
