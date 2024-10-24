namespace Backend.Domain.DTOs;

public class InterviewDetailsDto
{
    public Guid SessionId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public List<InterviewQuestionDetailDto> Questions { get; set; }
}
