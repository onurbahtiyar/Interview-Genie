using Backend.Domain.DTOs;
using Backend.Domain.Entities;

namespace Backend.Application.Interfaces;

public interface IInterviewService
{
    Task<InterviewSession> CreateInterviewAsync(Guid userId, CreateInterviewRequest request);
    Task<InterviewQuestionResponse> GetNextQuestionAsync(Guid interviewId);
    Task SubmitAnswerAsync(Guid interviewId, SubmitAnswerRequest request);
    Task<EndInterviewResponse> EndInterviewAsync(Guid interviewId);
    Task<MainPageDto> GetMainPageDataAsync(Guid userId);
    Task<InterviewDetailsDto> GetInterviewDetailsAsync(Guid interviewId);
}
