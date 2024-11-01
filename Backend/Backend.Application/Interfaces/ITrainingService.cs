using Backend.Domain.DTOs;

namespace Backend.Application.Interfaces;

public interface ITrainingService
{
    Task<TrainingPlan> GetTrainingPlanAsync(string topic, string language);
    Task<string> GetChatbotResponseAsync(string topic, string userQuestion, string language);
}