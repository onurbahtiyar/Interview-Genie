using Backend.Application.Interfaces;
using Backend.Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Kullanıcı kayıt işlemi
    /// </summary>
    /// <param name="registerDto">Kayıt bilgileri</param>
    /// <returns>Yeni kullanıcı bilgileri</returns>
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        try
        {
            var user = await _userService.RegisterAsync(registerDto);
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kullanıcı kaydı sırasında hata oluştu.");
            return StatusCode(500, new { message = "Kayıt sırasında bir hata oluştu. Lütfen tekrar deneyiniz." });
        }
    }

    /// <summary>
    /// Kullanıcı giriş işlemi
    /// </summary>
    /// <param name="loginDto">Giriş bilgileri</param>
    /// <returns>JWT Token ve kullanıcı bilgileri</returns>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            var response = await _userService.LoginAsync(loginDto);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kullanıcı girişi sırasında hata oluştu.");
            return StatusCode(500, new { message = "Giriş sırasında bir hata oluştu. Lütfen tekrar deneyiniz." });
        }
    }
}
