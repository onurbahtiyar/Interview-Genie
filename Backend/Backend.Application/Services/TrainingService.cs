using Backend.Application.Interfaces;
using Backend.Domain.DTOs;

namespace Backend.Application.Services;

public class TrainingService : ITrainingService
{
    private readonly IGeminiService _geminiService;

    public TrainingService(IGeminiService geminiService)
    {
        _geminiService = geminiService;
    }

    public async Task<TrainingPlan> GetTrainingPlanAsync(string topic, string language)
    {
        var trainingPlan = await _geminiService.GenerateTrainingPlanAsync(topic, language);
        return trainingPlan;
    }

    public async Task<string> GetChatbotResponseAsync(string topic, string userQuestion, string language)
    {
        var response = await _geminiService.ChatbotResponseAsync(topic, userQuestion, language);
        return response;
    }
}
