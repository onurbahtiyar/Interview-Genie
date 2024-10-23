namespace Backend.Domain.DTOs;

public class EndInterviewResponse
{
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public int IncorrectAnswers { get; set; }
}
