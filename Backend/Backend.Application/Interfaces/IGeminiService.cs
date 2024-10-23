using Backend.Domain.DTOs;
using Backend.Domain.Entities;

namespace Backend.Application.Interfaces;

public interface IGeminiService
{
    Task<List<InterviewQuestion>> GenerateInterviewQuestionsAsync(CreateInterviewRequest companyInfo);

}
