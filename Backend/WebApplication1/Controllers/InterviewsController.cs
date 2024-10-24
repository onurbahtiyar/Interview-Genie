using Backend.Application.Interfaces;
using Backend.Domain.DTOs;
using Backend.Infrastructure.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Backend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InterviewsController : ControllerBase
{
    private readonly IInterviewService _interviewService;

    public InterviewsController(IInterviewService interviewService)
    {
        _interviewService = interviewService;
    }

    /// <summary>
    /// Yeni bir görüşme oturumu oluşturur.
    /// </summary>
    /// <param name="request">Görüşme oluşturma isteği bilgileri.</param>
    /// <returns>Oluşturulan görüşme oturumunun kimliği.</returns>
    [HttpPost]
    public async Task<IActionResult> CreateInterview([FromBody] CreateInterviewRequest request)
    {
        var userId = Guid.Parse(User.Identity.Name);
        var interview = await _interviewService.CreateInterviewAsync(userId, request);
        return Ok(new { interview.Id });
    }

    /// <summary>
    /// Belirtilen görüşme oturumundaki bir sonraki soruyu getirir.
    /// </summary>
    /// <param name="interviewId">Görüşme oturumunun kimliği.</param>
    /// <returns>Bir sonraki görüşme sorusu.</returns>
    [HttpGet("{interviewId}/NextQuestion")]
    public async Task<IActionResult> GetNextQuestion(Guid interviewId)
    {
        try
        {
            var question = await _interviewService.GetNextQuestionAsync(interviewId);
            return Ok(question);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Belirtilen görüşme oturumuna bir cevap gönderir.
    /// </summary>
    /// <param name="interviewId">Görüşme oturumunun kimliği.</param>
    /// <param name="request">Cevap gönderme isteği bilgileri.</param>
    /// <returns>Cevabın başarıyla gönderildiğine dair mesaj.</returns>
    [HttpPost("{interviewId}/SubmitAnswer")]
    public async Task<IActionResult> SubmitAnswer(Guid interviewId, [FromBody] SubmitAnswerRequest request)
    {
        try
        {
            await _interviewService.SubmitAnswerAsync(interviewId, request);
            return Ok(new { message = "Cevap başarıyla gönderildi." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Belirtilen görüşme oturumunu sonlandırır ve sonuçları döner.
    /// </summary>
    /// <param name="interviewId">Görüşme oturumunun kimliği.</param>
    /// <returns>Görüşme sonuçları bilgisi.</returns>
    [HttpPost("{interviewId}/End")]
    public async Task<IActionResult> EndInterview(Guid interviewId)
    {
        try
        {
            var result = await _interviewService.EndInterviewAsync(interviewId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Kullanıcının ana sayfa verilerini alır, geçmiş görüşmeleri ve özet istatistikleri içerir.
    /// </summary>
    /// <returns>Ana sayfa verileri.</returns>
    [HttpGet("main")]
    public async Task<IActionResult> GetMainPageData()
    {
        try
        {
            var userId = User.GetUserId();
            var mainPageData = await _interviewService.GetMainPageDataAsync(userId);
            return Ok(mainPageData);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Belirtilen görüşme oturumunun detaylarını alır.
    /// </summary>
    /// <param name="interviewId">Görüşme oturumunun kimliği.</param>
    /// <returns>Görüşme oturumu detayları.</returns>
    [HttpGet("{interviewId}/details")]
    public async Task<IActionResult> GetInterviewDetails(Guid interviewId)
    {
        try
        {
            var details = await _interviewService.GetInterviewDetailsAsync(interviewId);
            return Ok(details);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
