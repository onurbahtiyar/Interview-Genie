using Backend.Application.Interfaces;
using Backend.Domain.DTOs;
using Backend.Domain.Entities;
using Backend.Domain.Enums;
using Backend.Repository;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Backend.Application.Services
{
    public class InterviewService : IInterviewService
    {
        private readonly IGeminiService _geminiService;
        private readonly IProfileService _profileService;
        private readonly IRepository<InterviewSession> _interviewRepository;
        private readonly IRepository<InterviewQuestion> _questionRepository;
        private readonly IRepository<CompanyInfo> _companyRepository;

        public InterviewService(
            IGeminiService geminiService,
            IProfileService profileService,
            IRepository<InterviewSession> interviewRepository,
            IRepository<InterviewQuestion> questionRepository,
            IRepository<CompanyInfo> companyRepository)
        {
            _geminiService = geminiService;
            _profileService = profileService;
            _interviewRepository = interviewRepository;
            _questionRepository = questionRepository;
            _companyRepository = companyRepository;
        }

        public async Task<InterviewSession> CreateInterviewAsync(Guid userId, CreateInterviewRequest request)
        {
            // 1. Şirket Bilgilerini Oluştur ve Kaydet
            var companyInfo = new CompanyInfo
            {
                Id = Guid.NewGuid(),
                CompanyName = request.CompanyName,
                Industry = request.Industry,
                Location = request.Location,
                Description = request.Description
            };

            await _companyRepository.AddAsync(companyInfo);

            // 2. Mülakat Oturumunu Oluştur (ProfileComment henüz boş)
            var interviewSession = new InterviewSession
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CompanyInfoId = companyInfo.Id,
                CompanyInfo = companyInfo,
                IsActive = true,
                ProfileComment = null, // Henüz analiz yapılmadı
                InterviewQuestions = new List<InterviewQuestion>()
            };

            await _interviewRepository.AddAsync(interviewSession);

            // 3. Değişiklikleri Kaydet (InterviewSession artık veritabanında mevcut)
            await _interviewRepository.SaveChangesAsync();

            // 4. Gemini API Kullanarak Soruları Üret
            var questions = await _geminiService.GenerateInterviewQuestionsAsync(request);

            foreach (var question in questions)
            {
                var interviewQuestion = new InterviewQuestion
                {
                    Id = question.Id,
                    InterviewSessionId = interviewSession.Id,
                    QuestionText = question.QuestionText,
                    QuestionType = question.QuestionType,
                    Options = question.Options,
                    CorrectAnswer = question.CorrectAnswer,
                    Topic = question.Topic
                };
                await _questionRepository.AddAsync(interviewQuestion);
                interviewSession.InterviewQuestions.Add(interviewQuestion);
            }

            // 5. Soruları ekledikten sonra değişiklikleri kaydet
            await _interviewRepository.SaveChangesAsync();

            return interviewSession;
        }

        public async Task<InterviewQuestionResponse> GetNextQuestionAsync(Guid interviewId)
        {
            var interview = await _interviewRepository.GetAsync(
                x => x.Id == interviewId,
                x => x.InterviewQuestions
            );

            if (interview == null || !interview.IsActive)
                throw new Exception("Interview session not found or inactive.");

            var nextQuestion = interview.InterviewQuestions.FirstOrDefault(q => q.UserAnswer == null);
            if (nextQuestion == null)
                throw new Exception("No more questions available.");

            _questionRepository.Update(nextQuestion);
            await _interviewRepository.SaveChangesAsync();

            return new InterviewQuestionResponse
            {
                QuestionId = nextQuestion.Id,
                QuestionText = nextQuestion.QuestionText,
                QuestionType = nextQuestion.QuestionType.ToString(),
                Options = nextQuestion.Options
            };
        }

        public async Task SubmitAnswerAsync(Guid interviewId, SubmitAnswerRequest request)
        {
            var interview = await _interviewRepository.GetAsync(
                x => x.Id == interviewId,
                x => x.InterviewQuestions
            );

            if (interview == null || !interview.IsActive)
                throw new Exception("Interview session not found or inactive.");

            var question = interview.InterviewQuestions.FirstOrDefault(q => q.Id == request.QuestionId);
            if (question == null)
                throw new Exception("Question not found in this interview session.");

            if (question.UserAnswer != null)
                throw new Exception("Question already answered.");

            // Çoktan seçmeli sorular için, seçeneğin geçerli olup olmadığını kontrol et
            if (question.QuestionType == QuestionType.MultipleChoice)
            {
                if (!question.Options.Contains(request.Answer))
                    throw new Exception("Invalid option selected.");
            }

            // Yanıtı kaydet
            question.UserAnswer = request.Answer;

            // Doğruluk kontrolü
            if (question.QuestionType == QuestionType.MultipleChoice)
            {
                question.IsCorrect = string.Equals(question.CorrectAnswer, request.Answer, StringComparison.OrdinalIgnoreCase);
            }
            else if (question.QuestionType == QuestionType.OpenEnded)
            {
                // Gemini ile kontrol
                question.IsCorrect = await _geminiService.CheckAnswerWithGeminiAsync(question.QuestionText, request.Answer, question.CorrectAnswer);
            }

            _questionRepository.Update(question);
            await _interviewRepository.SaveChangesAsync();
        }

        public async Task<EndInterviewResponse> EndInterviewAsync(Guid interviewId)
        {
            var interview = await _interviewRepository.GetAsync(
                x => x.Id == interviewId,
                x => x.InterviewQuestions
            );

            if (interview == null || !interview.IsActive)
                throw new Exception("Interview session not found or already ended.");

            interview.IsActive = false;

            var analysis = await AnalyzeInterviewWithProfileAsync(interview.UserId, interview.Id);

            interview.ProfileComment = JsonSerializer.Serialize(analysis, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            _interviewRepository.Update(interview);

            var total = interview.InterviewQuestions.Count;
            var correct = interview.InterviewQuestions.Count(q => q.IsCorrect == true);
            var incorrect = interview.InterviewQuestions.Count(q => q.IsCorrect == false);

            await _interviewRepository.SaveChangesAsync();

            return new EndInterviewResponse
            {
                TotalQuestions = total,
                CorrectAnswers = correct,
                IncorrectAnswers = incorrect
            };
        }

        public async Task<MainPageDto> GetMainPageDataAsync(Guid userId)
        {
            // Geçmiş süreçleri (bitti olanlar)
            var pastSessions = await _interviewRepository.GetAllAsync(
                s => s.UserId == userId && !s.IsActive,
                s => s.CompanyInfo,
                s => s.InterviewQuestions
            );

            // Verileri belleğe alıyoruz
            var pastSessionsList = pastSessions.ToList();

            var sessionDtos = pastSessionsList.Select(s => new InterviewSessionDto
            {
                Id = s.Id,
                UserId = s.UserId,
                CompanyName = s.CompanyInfo.CompanyName,
                IsActive = s.IsActive,
                TotalQuestions = s.InterviewQuestions.Count,
                CorrectAnswers = s.InterviewQuestions.Count(q => q.IsCorrect == true),
                IncorrectAnswers = s.InterviewQuestions.Count(q => q.IsCorrect == false)
            }).ToList();

            // Özet istatistikler
            var totalSessions = pastSessionsList.Count();
            var activeSessions = await _interviewRepository.CountAsync(s => s.UserId == userId && s.IsActive);
            var completedSessions = totalSessions;
            var averageCorrectAnswers = totalSessions > 0
                ? pastSessionsList.Average(s => s.InterviewQuestions.Count(q => q.IsCorrect == true))
                : 0;

            var summary = new InterviewSummaryDto
            {
                TotalSessions = totalSessions,
                ActiveSessions = activeSessions,
                CompletedSessions = completedSessions,
                AverageCorrectAnswers = averageCorrectAnswers
            };

            return new MainPageDto
            {
                PastSessions = sessionDtos,
                Summary = summary
            };
        }

        public async Task<InterviewDetailsDto> GetInterviewDetailsAsync(Guid interviewId)
        {
            var interview = await _interviewRepository.GetAsync(
                x => x.Id == interviewId,
                x => x.InterviewQuestions,
                x => x.CompanyInfo
            );

            if (interview == null)
                throw new Exception("Interview session not found.");

            var questionDetails = interview.InterviewQuestions.Select(q => new InterviewQuestionDetailDto
            {
                QuestionId = q.Id,
                QuestionText = q.QuestionText,
                QuestionType = q.QuestionType.ToString(),
                Options = q.Options,
                UserAnswer = q.UserAnswer,
                CorrectAnswer = q.CorrectAnswer,
                IsCorrect = q.IsCorrect ?? false,
                Topic = q.Topic
            }).ToList();

            return new InterviewDetailsDto
            {
                SessionId = interview.Id,
                ProfileComment = interview.ProfileComment,
                Questions = questionDetails
            };
        }

        private async Task<InterviewAnalysisDto> AnalyzeInterviewWithProfileAsync(Guid userId, Guid interviewId)
        {
            // Retrieve the user's profile
            var profileResult = await _profileService.GetProfileAsync(userId);
            if (!profileResult.Success)
            {
                throw new Exception("Failed to retrieve user profile.");
            }
            var profile = profileResult.Data;

            // Retrieve the interview session
            var interview = await _interviewRepository.GetAsync(
                x => x.Id == interviewId && x.UserId == userId,
                x => x.InterviewQuestions,
                x => x.CompanyInfo
            );
            if (interview == null)
            {
                throw new Exception("Interview session not found.");
            }

            // Call GeminiService to analyze the data
            var analysis = await _geminiService.AnalyzeInterviewWithProfileAsync(profile, interview);

            return analysis;
        }
    }

}