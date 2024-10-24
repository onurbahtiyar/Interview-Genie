using Backend.Application.Interfaces;
using Backend.Domain.DTOs;
using Backend.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Backend.Application.Services
{
    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _geminiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";
        private readonly ILogger<GeminiService> _logger;

        public GeminiService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiService> logger)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Gemini:ApiKey"];
            _logger = logger;
        }

        public async Task<List<InterviewQuestion>> GenerateInterviewQuestionsAsync(CreateInterviewRequest companyInfo)
        {
            var prompt = $"Aşağıdaki gerekli yetenekler ve iş tanımına dayanarak, adayın karşılaşabileceği potansiyel görüşme sorularının bir listesini oluştur. " +
                         $"Her soru, verilen yetenekler listesinden bir spesifik yeteneğe odaklanmalıdır. " +
                         $"Her soru için, 'topic' alanını sorunun ilgili olduğu yeteneğe ayarla (örneğin, soru .NET Core hakkındaysa, topic '.NET Core' olmalı). " +
                         $"Şirket hakkında sorular dahil etme. " +
                         $"{companyInfo.Language} dilinde soruları oluştur. " +
                         $"Gerekli Yetenekler: {string.Join(", ", companyInfo.Skills)}. " +
                         $"İş Tanımı: {companyInfo.Description}. " +
                         $"Çoktan seçmeli sorular için, seçenekleri açıklama olmadan ver ve doğru cevabı belirterek sadece bir seçeneği doğru cevap olarak işaretle. " +
                         $"Soruları aşağıdaki yapıya sahip JSON formatında hazırla:\n\n" +
                         "{\n" +
                         "  \"questions\": [\n" +
                         "    {\n" +
                         "      \"questionText\": \"Gerekli yeteneklerden biriyle ilgili bir soru, örneğin .NET Core mimarisi veya ilgili bir konu\",\n" +
                         "      \"questionType\": \"MultipleChoice\",\n" +
                         "      \"options\": [\"Seçenek1\", \"Seçenek2\", \"Seçenek3\", \"Seçenek4\"],\n" +
                         "      \"correctAnswer\": \"Seçenek1\",\n" +
                         "      \"topic\": \".NET Core\"\n" +
                         "    },\n" +
                         "    {\n" +
                         "      \"questionText\": \"Farklı bir gerekli yetenekle ilgili başka bir soru, örneğin Angular bileşenleri\",\n" +
                         "      \"questionType\": \"MultipleChoice\",\n" +
                         "      \"options\": [\"Seçenek1\", \"Seçenek2\", \"Seçenek3\", \"Seçenek4\"],\n" +
                         "      \"correctAnswer\": \"Seçenek2\",\n" +
                         "      \"topic\": \"Angular\"\n" +
                         "    },\n" +
                         "    {\n" +
                         "      \"questionText\": \"İş tanımındaki spesifik bir gerekli yetenek veya zorluk hakkında açık uçlu bir soru\",\n" +
                         "      \"questionType\": \"OpenEnded\",\n" +
                         "      \"correctAnswer\": \"Yeteneğe veya role ilişkin beklenen cevap\",\n" +
                         "      \"topic\": \"Yetenek İsmi\"\n" +
                         "    }\n" +
                         "  ]\n" +
                         "}";


            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    response_mime_type = "application/json",
                    response_schema = new
                    {
                        type = "OBJECT",
                        properties = new
                        {
                            questions = new
                            {
                                type = "ARRAY",
                                items = new
                                {
                                    type = "OBJECT",
                                    properties = new
                                    {
                                        questionText = new { type = "STRING" },
                                        questionType = new { type = "STRING" },
                                        options = new
                                        {
                                            type = "ARRAY",
                                            items = new { type = "STRING" }
                                        },
                                        correctAnswer = new { type = "STRING" },
                                        topic = new { type = "STRING" }
                                    },
                                    required = new[] { "questionText", "questionType", "correctAnswer" }
                                }
                            }
                        }
                    }
                }
            };

            var jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_geminiEndpoint}?key={_apiKey}", content);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            _logger.LogInformation($"Gemini API Response: {jsonResponse}");

            response.EnsureSuccessStatusCode();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var wrapper = JsonSerializer.Deserialize<GeminiResponseWrapper>(jsonResponse, options);

            if (wrapper?.Candidates == null || !wrapper.Candidates.Any())
            {
                _logger.LogError("Invalid response from Gemini API: No candidates found.");
                throw new JsonException("Invalid response from Gemini API: No candidates found.");
            }

            var firstCandidate = wrapper.Candidates.FirstOrDefault();
            if (firstCandidate?.Content?.Parts == null || !firstCandidate.Content.Parts.Any())
            {
                _logger.LogError("Invalid response from Gemini API: No content parts found.");
                throw new JsonException("Invalid response from Gemini API: No content parts found.");
            }

            var responseText = firstCandidate.Content.Parts.First().Text;

            var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseText, options);

            if (geminiResponse?.Questions == null || !geminiResponse.Questions.Any())
            {
                _logger.LogError("Invalid response from Gemini API: No questions found.");
                throw new JsonException("Invalid response from Gemini API: No questions found.");
            }

            var interviewQuestions = new List<InterviewQuestion>();

            foreach (var geminiQuestion in geminiResponse.Questions)
            {
                if (!Enum.TryParse<QuestionType>(geminiQuestion.QuestionType, true, out var questionType))
                {
                    _logger.LogWarning($"Unknown question type: {geminiQuestion.QuestionType}. Defaulting to OpenEnded.");
                    questionType = QuestionType.OpenEnded;
                }

                var interviewQuestion = new InterviewQuestion
                {
                    Id = Guid.NewGuid(),
                    QuestionText = geminiQuestion.QuestionText,
                    QuestionType = questionType,
                    CorrectAnswer = geminiQuestion.CorrectAnswer,
                    Topic = geminiQuestion.Topic
                };
                if (questionType == QuestionType.MultipleChoice)
                {
                    interviewQuestion.Options = geminiQuestion.Options ?? new List<string>();
                }

                interviewQuestions.Add(interviewQuestion);
            }

            return interviewQuestions;
        }
    }
}
