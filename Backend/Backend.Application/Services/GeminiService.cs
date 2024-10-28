using Backend.Application.Interfaces;
using Backend.Domain.DTOs;
using Backend.Domain.Entities;
using Backend.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Backend.Application.Services;

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
        var summary = $"{companyInfo.Description}. ";
        foreach (var item in companyInfo.Skills)
        {
            summary += $"{item}\n";
        }

        // 1. Konuları Üretme
        var topics = await GenerateTopicsAsync(summary, companyInfo.Language);

        // Her konu için oluşturulacak soru sayısı
        int questionsPerTopic = 5; // 4 MultipleChoice + 1 OpenEnded

        // 2. Tüm konular için soruları tek seferde üretme
        var allInterviewQuestions = await GenerateQuestionsForTopicsAsync(topics, questionsPerTopic, companyInfo.Language, companyInfo.Description);

        return allInterviewQuestions;
    }

    private async Task<List<string>> GenerateTopicsAsync(string summary, string language)
    {
        var prompt = $"Aşağıdaki özet metne dayanarak, doğrudan ilgili en fazla 6 konu başlığının bir listesini oluştur. " +
                     $"Konu başlıklarını bir JSON dizisi olarak sadece konu adlarını içerecek şekilde ver. " +
                     $"Ek açıklama veya numaralandırma ekleme. " +
                     $"Yanıtı JSON formatında ver.\n\n" +
                     $"Özet: {summary}\n\n" +
                     $"JSON formatında konu başlıkları listesini {language} dilinde hazırla.";

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
            }
        };

        var jsonRequest = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_geminiEndpoint}?key={_apiKey}", content);
        var jsonResponse = await response.Content.ReadAsStringAsync();

        _logger.LogInformation($"Gemini API Response (Topics): {jsonResponse}");

        response.EnsureSuccessStatusCode();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var wrapper = JsonSerializer.Deserialize<GeminiResponseWrapper>(jsonResponse, options);

        if (wrapper?.Candidates == null || !wrapper.Candidates.Any())
        {
            _logger.LogError("Invalid response from Gemini API: No candidates found for topics.");
            throw new JsonException("Invalid response from Gemini API: No candidates found for topics.");
        }

        var firstCandidate = wrapper.Candidates.FirstOrDefault();
        if (firstCandidate?.Content?.Parts == null || !firstCandidate.Content.Parts.Any())
        {
            _logger.LogError("Invalid response from Gemini API: No content parts found for topics.");
            throw new JsonException("Invalid response from Gemini API: No content parts found for topics.");
        }

        var responseText = firstCandidate.Content.Parts.First().Text;

        // JSON dizisini ayrıştırma
        List<string> topics;
        try
        {
            topics = JsonSerializer.Deserialize<List<string>>(responseText, options);
        }
        catch (JsonException ex)
        {
            _logger.LogError($"Failed to parse topics JSON: {ex.Message}");
            throw;
        }

        // Maksimum 6 konu ile sınırlama
        if (topics.Count > 6)
        {
            topics = topics.Take(6).ToList();
            _logger.LogWarning("Gemini API returned more than 6 topics. Trimming to first 6 topics.");
        }

        _logger.LogInformation($"Generated Topics: {string.Join(", ", topics)}");

        return topics;
    }

    private async Task<List<InterviewQuestion>> GenerateQuestionsForTopicsAsync(List<string> topics, int questionsPerTopic, string language, string jobDescription)
    {
        // Toplam soru sayısı: topics.Count * questionsPerTopic
        var prompt = $"Aşağıdaki iş tanımı ve konu başlıklarına dayanarak, her konu başlığı için {questionsPerTopic} adet adayın karşılaşabileceği potansiyel görüşme sorularının bir listesini oluştur. " +
                     $"Her konu başlığı için 1 adet 'OpenEnded' ve 4 adet 'MultipleChoice' soru oluşturulmalıdır. " +
                     $"Her soru, ilgili olduğu konu başlığına odaklanmalıdır ve 'topic' alanında bu konu başlığını belirtmelidir. " +
                     $"Soruların zorluk derecesi iş tanımına göre ayarlanmalıdır. " +
                     $"Şirket hakkında sorular dahil etme. " +
                     $"{language} dilinde soruları oluştur. " +
                     $"İş Tanımı: {jobDescription}. " +
                     $"Konu Başlıkları: {string.Join(", ", topics)}. " +
                     $"Çoktan seçmeli sorular için, seçenekleri açıklama olmadan ver ve doğru cevabı belirterek sadece bir seçeneği doğru cevap olarak işaretle. " +
                     $"Her konu başlığı için 1 'OpenEnded' ve 4 'MultipleChoice' soruyu aşağıdaki yapıya sahip JSON formatında hazırla:\n\n" +
                     "{\n" +
                     "  \"questions\": [\n" +
                     "    {\n" +
                     "      \"questionText\": \"Soru metni\",\n" +
                     "      \"questionType\": \"MultipleChoice\",\n" +
                     "      \"options\": [\"Seçenek1\", \"Seçenek2\", \"Seçenek3\", \"Seçenek4\"],\n" +
                     "      \"correctAnswer\": \"Seçenek1\",\n" +
                     "      \"topic\": \"İlgili konu başlığı\"\n" +
                     "    },\n" +
                     "    {\n" +
                     "      \"questionText\": \"Soru metni\",\n" +
                     "      \"questionType\": \"OpenEnded\",\n" +
                     "      \"topic\": \"İlgili konu başlığı\"\n" +
                     "    }\n" +
                     "    // ... daha fazla soru\n" +
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
                                required = new[] { "questionText", "questionType", "correctAnswer", "topic" }
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

        _logger.LogInformation($"Gemini API Response (Questions for Topics): {jsonResponse}");

        response.EnsureSuccessStatusCode();

        var optionsSerializer = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var wrapper = JsonSerializer.Deserialize<GeminiResponseWrapper>(jsonResponse, optionsSerializer);

        if (wrapper?.Candidates == null || !wrapper.Candidates.Any())
        {
            _logger.LogError($"Invalid response from Gemini API: No candidates found for topics.");
            throw new JsonException($"Invalid response from Gemini API: No candidates found for topics.");
        }

        var firstCandidate = wrapper.Candidates.FirstOrDefault();
        if (firstCandidate?.Content?.Parts == null || !firstCandidate.Content.Parts.Any())
        {
            _logger.LogError($"Invalid response from Gemini API: No content parts found for topics.");
            throw new JsonException($"Invalid response from Gemini API: No content parts found for topics.");
        }

        var responseText = firstCandidate.Content.Parts.First().Text;

        GeminiResponse geminiResponse;
        try
        {
            geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseText, optionsSerializer);
        }
        catch (JsonException ex)
        {
            _logger.LogError($"Failed to parse questions JSON: {ex.Message}");
            throw;
        }

        if (geminiResponse?.Questions == null || !geminiResponse.Questions.Any())
        {
            _logger.LogError($"Invalid response from Gemini API: No questions found for topics.");
            throw new JsonException($"Invalid response from Gemini API: No questions found for topics.");
        }

        // Doğrulama: Her konu için doğru sayıda ve türde soru var mı?
        var groupedQuestions = geminiResponse.Questions.GroupBy(q => q.Topic);
        var interviewQuestions = new List<InterviewQuestion>();

        foreach (var group in groupedQuestions)
        {
            var topic = group.Key;
            var questions = group.ToList();

            // Beklenen soru sayısı: 5 (4 MC + 1 OE)
            if (questions.Count != questionsPerTopic)
            {
                _logger.LogError($"Topic '{topic}' için beklenen soru sayısı {questionsPerTopic}, fakat {questions.Count} soru bulundu.");
                throw new JsonException($"Topic '{topic}' için beklenen soru sayısı {questionsPerTopic}, fakat {questions.Count} soru bulundu.");
            }

            // Soru türlerinin doğruluğunu kontrol et
            var multipleChoiceCount = questions.Count(q => q.QuestionType.Equals("MultipleChoice", StringComparison.OrdinalIgnoreCase));
            var openEndedCount = questions.Count(q => q.QuestionType.Equals("OpenEnded", StringComparison.OrdinalIgnoreCase));

            //if (multipleChoiceCount != 4 || openEndedCount != 1)
            //{
            //    _logger.LogError($"Topic '{topic}' için beklenen 4 MultipleChoice ve 1 OpenEnded soru bulunamadı.");
            //    throw new JsonException($"Topic '{topic}' için beklenen 4 MultipleChoice ve 1 OpenEnded soru bulunamadı.");
            //}

            foreach (var geminiQuestion in questions)
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
                    interviewQuestion.Options = geminiQuestion.Options ?? Array.Empty<string>();
                }

                interviewQuestions.Add(interviewQuestion);
            }
        }

        _logger.LogInformation($"Generated Interview Questions: {interviewQuestions.Count}");

        return interviewQuestions;
    }

    public async Task<bool> CheckAnswerWithGeminiAsync(string questionText, string userAnswer, string correctAnswer)
    {
        var prompt = $"Aşağıdaki soru ve cevaba dayanarak, kullanıcının cevabının doğru olup olmadığını 'Evet' veya 'Hayır' olarak belirt.\n\n" +
                     $"Soru: {questionText}\n" +
                     $"Doğru Cevap: {correctAnswer}\n" +
                     $"Kullanıcının Cevabı: {userAnswer}\n\n" +
                     $"Cevabın Doğruluğu:";

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
            }
        };

        var jsonRequest = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_geminiEndpoint}?key={_apiKey}", content);
        var jsonResponse = await response.Content.ReadAsStringAsync();

        _logger.LogInformation($"Gemini API Response (Answer Check): {jsonResponse}");

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"Gemini API Error: {response.StatusCode} - {jsonResponse}");
            throw new Exception($"Gemini API Error: {response.StatusCode} - {jsonResponse}");
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var geminiResponse = JsonSerializer.Deserialize<GeminiResponseWrapper>(jsonResponse, options);

        if (geminiResponse == null || geminiResponse.Candidates == null || !geminiResponse.Candidates.Any())
        {
            _logger.LogError("Invalid response from Gemini API while checking answer.");
            throw new Exception("Invalid response from Gemini API while checking answer.");
        }

        var candidate = geminiResponse.Candidates.FirstOrDefault();
        if (candidate?.Content?.Parts == null || !candidate.Content.Parts.Any())
        {
            _logger.LogError("Invalid response from Gemini API: No content parts found for answer check.");
            throw new JsonException("Invalid response from Gemini API: No content parts found for answer check.");
        }

        var responseText = candidate.Content.Parts.First().Text;

        if (string.IsNullOrWhiteSpace(responseText))
        {
            _logger.LogError("Gemini API returned empty response text.");
            throw new Exception("Gemini API returned empty response text.");
        }

        var geminiOutput = responseText.Trim().ToLower();

        if (geminiOutput.Contains("evet"))
        {
            return true;
        }
        else if (geminiOutput.Contains("hayır"))
        {
            return false;
        }
        else
        {
            _logger.LogWarning($"Gemini API returned an unexpected response: {geminiOutput}");
            throw new Exception($"Gemini API returned an unexpected response: {geminiOutput}");
        }
    }

    public async Task<InterviewAnalysisDto> AnalyzeInterviewWithProfileAsync(ProfileDto profile, InterviewSession interview)
    {
        // Profil ve mülakat verilerini JSON formatına çevirme
        var profileJson = JsonSerializer.Serialize(profile);
        var interviewJson = JsonSerializer.Serialize(new
        {
            interview.Id,
            interview.UserId,
            interview.IsActive,
            CompanyInfo = new
            {
                interview.CompanyInfo.Id,
                interview.CompanyInfo.CompanyName,
                interview.CompanyInfo.Industry,
                interview.CompanyInfo.Location,
                interview.CompanyInfo.Description
            }
        });

        // Kullanıcının yanlış cevap verdiği soruları konularına göre grupla
        var incorrectAnswersByTopic = interview.InterviewQuestions
            .Where(q => q.IsCorrect == false)
            .GroupBy(q => q.Topic)
            .Select(g => new
            {
                Topic = g.Key,
                IncorrectCount = g.Count()
            })
            .ToList();

        var incorrectAnswersJson = JsonSerializer.Serialize(incorrectAnswersByTopic);

        var prompt = $"Kullanıcının profili, tamamladığı mülakat ve yanlış cevap verdiği konular aşağıda verilmiştir. " +
                     $"Profiline ve mülakat performansına dayanarak kullanıcının gelişebileceği alanları belirle. " +
                     $"Eksik olduğu konuları ve bu konularda nasıl gelişebileceğini detaylı bir şekilde açıkla. " +
                     $"Yanıtını aşağıdaki JSON formatında ver:\n\n" +
                     "{\n" +
                     "  \"areasOfImprovement\": [\n" +
                     "    {\n" +
                     "      \"topic\": \"Konu Adı\",\n" +
                     "      \"feedback\": \"Bu konuda gelişmek için yapabilecekleri.\"\n" +
                     "    }\n" +
                     "    // ... daha fazla konu\n" +
                     "  ],\n" +
                     "  \"overallFeedback\": \"Genel geri bildirim.\"\n" +
                     "}\n\n" +
                     $"Kullanıcı Profili:\n{profileJson}\n\n" +
                     $"Mülakat Verisi:\n{interviewJson}\n\n" +
                     $"Yanlış Cevap Verilen Konular:\n{incorrectAnswersJson}";

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
            }
        };

        var jsonRequest = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_geminiEndpoint}?key={_apiKey}", content);
        var jsonResponse = await response.Content.ReadAsStringAsync();

        _logger.LogInformation($"Gemini API Response (Interview Analysis): {jsonResponse}");

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"Gemini API Error: {response.StatusCode} - {jsonResponse}");
            throw new Exception($"Gemini API Error: {response.StatusCode} - {jsonResponse}");
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        GeminiResponseDto geminiResponse;
        try
        {
            geminiResponse = JsonSerializer.Deserialize<GeminiResponseDto>(jsonResponse, options);
            _logger.LogInformation($"Gemini API Parsed Response: {JsonSerializer.Serialize(geminiResponse)}");
        }
        catch (JsonException ex)
        {
            _logger.LogError($"Failed to parse Gemini response JSON: {ex.Message}");
            throw;
        }

        if (geminiResponse?.Candidates == null || !geminiResponse.Candidates.Any())
        {
            _logger.LogError("Gemini API returned no candidates.");
            throw new Exception("Gemini API returned no candidates.");
        }

        var analysisText = geminiResponse.Candidates[0].Content.Parts.FirstOrDefault()?.Text;

        _logger.LogInformation($"Extracted Analysis Text: {analysisText}");

        if (string.IsNullOrWhiteSpace(analysisText))
        {
            _logger.LogError("Gemini API returned empty analysis text.");
            throw new Exception("Gemini API returned empty analysis text.");
        }

        InterviewAnalysisDto analysis;
        try
        {
            analysis = JsonSerializer.Deserialize<InterviewAnalysisDto>(analysisText, options);
            _logger.LogInformation($"Deserialized Analysis: {JsonSerializer.Serialize(analysis)}");
        }
        catch (JsonException ex)
        {
            _logger.LogError($"Failed to parse analysis JSON: {ex.Message}");
            throw;
        }

        if (analysis == null)
        {
            _logger.LogError("Deserialized analysis is null.");
            throw new Exception("Deserialized analysis is null.");
        }

        return analysis;
    }

}
