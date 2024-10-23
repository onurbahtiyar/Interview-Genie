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
            var prompt = $"Based on the following company information, generate a list of potential interview questions that a candidate might encounter. " +
                         $"The questions should be aligned with the skills required for the job and company description. Please generate the questions in {companyInfo.Language}. " +
                         $"Company Info: " +
                         $"Company Name: {companyInfo.CompanyName}, " +
                         $"Industry: {companyInfo.Industry}, " +
                         $"Location: {companyInfo.Location}, " +
                         $"Required Skills: {string.Join(", ", companyInfo.Skills)}, " +
                         $"Job Description: {companyInfo.Description}. " +
                         $"For multiple-choice questions, provide options without explanations and indicate the correct answer by specifying exactly one option as the correct answer. " +
                         $"Format the questions in JSON with the following structure:\n\n" +
                         "{\n" +
                         "  \"questions\": [\n" +
                         "    {\n" +
                         "      \"questionText\": \"Question related to C# layered architecture or a relevant topic\",\n" +
                         "      \"questionType\": \"MultipleChoice\",\n" +
                         "      \"options\": [\"Blazor\", \"React\", \"Angular\", \"Vue\"],\n" +
                         "      \"correctAnswer\": \"Blazor\"\n" +
                         "    },\n" +
                         "    {\n" +
                         "      \"questionText\": \"Question related to Angular, Power BI or other mentioned skills\",\n" +
                         "      \"questionType\": \"MultipleChoice\",\n" +
                         "      \"options\": [\"Option1\", \"Option2\", \"Option3\", \"Option4\"],\n" +
                         "      \"correctAnswer\": \"Option1\"\n" +
                         "    },\n" +
                         "    {\n" +
                         "      \"questionText\": \"Open-ended question about a specific job requirement or challenge from the job description\",\n" +
                         "      \"questionType\": \"OpenEnded\",\n" +
                         "      \"correctAnswer\": \"Expected answer related to job role or skillset\"\n" +
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
                                        correctAnswer = new { type = "STRING" }
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
                    CorrectAnswer = geminiQuestion.CorrectAnswer
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
