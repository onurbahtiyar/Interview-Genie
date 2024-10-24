namespace Backend.Domain.DTOs;

public class InterviewSummaryDto
{
    public int TotalSessions { get; set; }
    public int ActiveSessions { get; set; }
    public int CompletedSessions { get; set; }
    public double AverageCorrectAnswers { get; set; }
}
