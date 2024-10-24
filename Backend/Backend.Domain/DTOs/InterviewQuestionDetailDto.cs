namespace Backend.Domain.DTOs;

public class InterviewQuestionDetailDto
{
    public Guid QuestionId { get; set; }
    public string QuestionText { get; set; }
    public string QuestionType { get; set; }
    public List<string> Options { get; set; }
    public string UserAnswer { get; set; }
    public string CorrectAnswer { get; set; }
    public bool IsCorrect { get; set; }
    public string Topic { get; set; }
}
