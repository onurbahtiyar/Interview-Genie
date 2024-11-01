namespace Backend.Domain.DTOs;

public class ChatbotRequest
{
    public string Topic { get; set; }
    public string UserQuestion { get; set; }
    public string Language { get; set; } = "Türkçe";
}
