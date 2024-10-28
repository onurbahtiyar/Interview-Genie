using Backend.Domain.DTOs;
using Backend.Domain.Entities;

namespace Backend.Application.Interfaces;

public interface IGeminiService
{
    Task<List<InterviewQuestion>> GenerateInterviewQuestionsAsync(CreateInterviewRequest companyInfo);
    Task<bool> CheckAnswerWithGeminiAsync(string questionText, string userAnswer, string correctAnswer);
    Task<InterviewAnalysisDto> AnalyzeInterviewWithProfileAsync(ProfileDto profile, InterviewSession interview);

}
