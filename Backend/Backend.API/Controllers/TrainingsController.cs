using Backend.Application.Interfaces;
using Backend.Domain.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Backend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TrainingsController : ControllerBase
{
    private readonly ITrainingService _trainingService;

    public TrainingsController(ITrainingService trainingService)
    {
        _trainingService = trainingService;
    }

    /// <summary>
    /// Kullanıcının gönderdiği konu hakkında eğitim planı oluşturur.
    /// </summary>
    /// <param name="request">Eğitim planı isteği bilgileri.</param>
    /// <returns>Oluşturulan eğitim planı.</returns>
    [HttpPost("GenerateTrainingPlan")]
    public async Task<IActionResult> GenerateTrainingPlan([FromBody] GenerateTrainingPlanRequest request)
    {
        try
        {
            var trainingPlan = await _trainingService.GetTrainingPlanAsync(request.Topic, request.Language);
            return Ok(trainingPlan);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Kullanıcının anlamadığı adım hakkında sohbet botu cevabı alır.
    /// </summary>
    /// <param name="request">Sohbet botu isteği bilgileri.</param>
    /// <returns>Sohbet botu cevabı.</returns>
    [HttpPost("Chatbot")]
    public async Task<IActionResult> Chatbot([FromBody] ChatbotRequest request)
    {
        try
        {
            var response = await _trainingService.GetChatbotResponseAsync(request.Topic, request.UserQuestion, request.Language);
            return Ok(new { response });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}