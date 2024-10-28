namespace Backend.Domain.DTOs;

public class InterviewDetailsDto
{
    public Guid SessionId { get; set; }
    public string? ProfileComment { get; set; }
    public List<InterviewQuestionDetailDto> Questions { get; set; }
}
