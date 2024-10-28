using Backend.Application.Interfaces;
using Backend.Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    /// <summary>
    /// Kullanıcının profilini getirir.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var userId = Guid.Parse(User.Identity.Name);
        var result = await _profileService.GetProfileAsync(userId);

        if (result.Success)
            return Ok(result);
        else
            return BadRequest(result);
    }

    /// <summary>
    /// Kullanıcının yeteneklerini günceller.
    /// </summary>
    [HttpPost("skills")]
    public async Task<IActionResult> UpdateSkills([FromBody] List<string> skills)
    {
        var userId = Guid.Parse(User.Identity.Name);
        var result = await _profileService.UpdateSkillsAsync(userId, skills);

        if (result.Success)
            return Ok(result);
        else
            return BadRequest(result);
    }

    /// <summary>
    /// Kullanıcının bildiği dilleri günceller.
    /// </summary>
    [HttpPost("languages")]
    public async Task<IActionResult> UpdateLanguages([FromBody] List<string> languages)
    {
        var userId = Guid.Parse(User.Identity.Name);
        var result = await _profileService.UpdateLanguagesAsync(userId, languages);

        if (result.Success)
            return Ok(result);
        else
            return BadRequest(result);
    }

    /// <summary>
    /// Kullanıcının projelerini ekler veya günceller.
    /// </summary>
    [HttpPost("projects")]
    public async Task<IActionResult> AddOrUpdateProjects([FromBody] List<ProjectDto> projects)
    {
        var userId = Guid.Parse(User.Identity.Name);
        var result = await _profileService.AddOrUpdateProjectsAsync(userId, projects);

        if (result.Success)
            return Ok(result);
        else
            return BadRequest(result);
    }

    /// <summary>
    /// Kullanıcının şirket bilgilerini ekler veya günceller.
    /// </summary>
    [HttpPost("companies")]
    public async Task<IActionResult> AddOrUpdateCompanies([FromBody] List<CompanyDto> companies)
    {
        var userId = Guid.Parse(User.Identity.Name);
        var result = await _profileService.AddOrUpdateCompaniesAsync(userId, companies);

        if (result.Success)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [HttpGet("skills/all")]
    public async Task<IActionResult> GetAllSkills()
    {
        var result = await _profileService.GetAllSkillsAsync();

        if (result.Success)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [HttpGet("languages/all")]
    public async Task<IActionResult> GetAllLanguages()
    {
        var result = await _profileService.GetAllLanguagesAsync();

        if (result.Success)
            return Ok(result);
        else
            return BadRequest(result);
    }
}
