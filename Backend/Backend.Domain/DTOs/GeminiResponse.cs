using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Backend.Domain.DTOs
{
    public class GeminiResponseWrapper
    {
        [JsonPropertyName("candidates")]
        public List<Candidate> Candidates { get; set; }
    }

    public class Candidate
    {
        [JsonPropertyName("content")]
        public Content Content { get; set; }
    }

    public class Content
    {
        [JsonPropertyName("parts")]
        public List<Part> Parts { get; set; }
    }

    public class Part
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }

    public class GeminiResponse
    {
        [JsonPropertyName("questions")]
        public List<GeminiQuestion> Questions { get; set; }
    }

    public class GeminiQuestion
    {
        [JsonPropertyName("questionText")]
        public string QuestionText { get; set; }

        [JsonPropertyName("questionType")]
        public string QuestionType { get; set; }

        [JsonPropertyName("options")]
        public List<string> Options { get; set; }

        [JsonPropertyName("correctAnswer")]
        public string CorrectAnswer { get; set; }
        [JsonPropertyName("topic")]
        public string Topic { get; set; }
    }
}
