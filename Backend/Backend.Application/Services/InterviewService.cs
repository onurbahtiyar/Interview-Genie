using Backend.Application.Interfaces;
using Backend.Domain.DTOs;
using Backend.Domain.Entities;
using Backend.Repository;

namespace Backend.Application.Services
{
    public class InterviewService : IInterviewService
    {
        private readonly IGeminiService _geminiService;
        private readonly IRepository<InterviewSession> _interviewRepository;
        private readonly IRepository<InterviewQuestion> _questionRepository;
        private readonly IRepository<CompanyInfo> _companyRepository;

        public InterviewService(
            IGeminiService geminiService,
            IRepository<InterviewSession> interviewRepository,
            IRepository<InterviewQuestion> questionRepository,
            IRepository<CompanyInfo> companyRepository)
        {
            _geminiService = geminiService;
            _interviewRepository = interviewRepository;
            _questionRepository = questionRepository;
            _companyRepository = companyRepository;
        }

        public async Task<InterviewSession> CreateInterviewAsync(Guid userId, CreateInterviewRequest request)
        {
            var companyInfo = new CompanyInfo
            {
                Id = Guid.NewGuid(),
                CompanyName = request.CompanyName,
                Industry = request.Industry,
                Location = request.Location,
                Description = request.Description
            };

            await _companyRepository.AddAsync(companyInfo);

            var interviewSession = new InterviewSession
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CompanyInfoId = companyInfo.Id,
                CompanyInfo = companyInfo,
                IsActive = true,
                Questions = new List<InterviewQuestion>(),
                StartedAt = DateTime.UtcNow
            };

            await _interviewRepository.AddAsync(interviewSession);

            // Gemini API kullanarak soruları üret
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
                    AskedAt = DateTime.UtcNow,
                    Topic = question.Topic
                };
                await _questionRepository.AddAsync(interviewQuestion);
                interviewSession.Questions.Add(interviewQuestion);
            }

            await _interviewRepository.SaveChangesAsync();

            return interviewSession;
        }

        public async Task<InterviewQuestionResponse> GetNextQuestionAsync(Guid interviewId)
        {
            var interview = await _interviewRepository.GetAsync(
                x => x.Id == interviewId,
                x => x.Questions
            );

            if (interview == null || !interview.IsActive)
                throw new Exception("Interview session not found or inactive.");

            var nextQuestion = interview.Questions.FirstOrDefault(q => q.UserAnswer == null);
            if (nextQuestion == null)
                throw new Exception("No more questions available.");

            nextQuestion.AskedAt = DateTime.UtcNow;
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
                x => x.Questions
            );

            if (interview == null || !interview.IsActive)
                throw new Exception("Interview session not found or inactive.");

            var question = interview.Questions.FirstOrDefault(q => q.Id == request.QuestionId);
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
            question.AnsweredAt = DateTime.UtcNow;

            // Doğruluk kontrolü
            if (question.QuestionType == QuestionType.MultipleChoice)
            {
                question.IsCorrect = string.Equals(question.CorrectAnswer, request.Answer, StringComparison.OrdinalIgnoreCase);
            }
            else if (question.QuestionType == QuestionType.OpenEnded)
            {
                // Açık uçlu sorular için basit bir doğruluk kontrolü
                question.IsCorrect = false; // Varsayılan olarak yanlış
                if (!string.IsNullOrWhiteSpace(question.CorrectAnswer))
                {
                    question.IsCorrect = request.Answer?.Contains(question.CorrectAnswer, StringComparison.OrdinalIgnoreCase) ?? false;
                }
            }

            _questionRepository.Update(question);
            await _interviewRepository.SaveChangesAsync();
        }

        public async Task<EndInterviewResponse> EndInterviewAsync(Guid interviewId)
        {
            var interview = await _interviewRepository.GetAsync(
                x => x.Id == interviewId,
                x => x.Questions
            );

            if (interview == null || !interview.IsActive)
                throw new Exception("Interview session not found or already ended.");

            interview.IsActive = false;
            interview.EndedAt = DateTime.UtcNow;
            _interviewRepository.Update(interview);

            var total = interview.Questions.Count;
            var correct = interview.Questions.Count(q => q.IsCorrect == true);
            var incorrect = interview.Questions.Count(q => q.IsCorrect == false);

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
                s => s.Questions
            );

            var sessionDtos = pastSessions.Select(s => new InterviewSessionDto
            {
                Id = s.Id,
                UserId = s.UserId,
                CompanyName = s.CompanyInfo.CompanyName,
                IsActive = s.IsActive,
                StartedAt = s.StartedAt,
                EndedAt = s.EndedAt,
                TotalQuestions = s.Questions.Count,
                CorrectAnswers = s.Questions.Count(q => q.IsCorrect == true),
                IncorrectAnswers = s.Questions.Count(q => q.IsCorrect == false)
            }).ToList();

            // Özet istatistikler
            var totalSessions = pastSessions.Count();
            var activeSessions = await _interviewRepository.CountAsync(s => s.UserId == userId && s.IsActive);
            var completedSessions = totalSessions;
            var averageCorrectAnswers = totalSessions > 0 ? pastSessions.Average(s => s.Questions.Count(q => q.IsCorrect == true)) : 0;

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
                x => x.Questions,
                x => x.CompanyInfo
            );

            if (interview == null)
                throw new Exception("Interview session not found.");

            var questionDetails = interview.Questions.Select(q => new InterviewQuestionDetailDto
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
                StartedAt = interview.StartedAt,
                EndedAt = interview.EndedAt,
                Questions = questionDetails
            };
        }


    }
}