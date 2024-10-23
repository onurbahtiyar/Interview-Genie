namespace Backend.Domain.DTOs;

public class SubmitAnswerRequest
{
    public Guid QuestionId { get; set; }
    public string Answer { get; set; }
}
