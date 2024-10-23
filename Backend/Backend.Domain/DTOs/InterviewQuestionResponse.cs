namespace Backend.Domain.DTOs;

public class InterviewQuestionResponse
{
    public Guid QuestionId { get; set; }
    public string QuestionText { get; set; }
    public string QuestionType { get; set; }
    public List<string> Options { get; set; }
}
