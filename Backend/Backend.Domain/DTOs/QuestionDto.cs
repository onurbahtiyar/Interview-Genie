namespace Backend.Domain.DTOs;

public class QuestionDto
{
    public string Id { get; set; }
    public string QuestionText { get; set; }
    public string QuestionType { get; set; }
    public string UserAnswer { get; set; }
    public string CorrectAnswer { get; set; }
    public bool IsCorrect { get; set; }
}
