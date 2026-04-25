using System.Security.Claims;
using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly AppDbContext _db;

    public SettingsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetSettings()
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var settings = await _db.UserSettings
            .FirstOrDefaultAsync(s => s.AccountId == userId.Value);

        if (settings == null)
        {
            settings = new UserSettings
            {
                AccountId = userId.Value
            };

            _db.UserSettings.Add(settings);
            await _db.SaveChangesAsync();
        }

        return Ok(new SettingsDto
        {
            DarkMode = settings.DarkMode,
            AutoEmailWhenLow = settings.AutoEmailWhenLow,
            LowLevelThreshold = settings.LowLevelThreshold
        });
    }

    [HttpPut]
    public async Task<IActionResult> UpdateSettings([FromBody] SettingsDto dto)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var settings = await _db.UserSettings
            .FirstOrDefaultAsync(s => s.AccountId == userId.Value);

        if (settings == null)
        {
            settings = new UserSettings
            {
                AccountId = userId.Value
            };

            _db.UserSettings.Add(settings);
        }

        settings.DarkMode = dto.DarkMode;
        settings.AutoEmailWhenLow = dto.AutoEmailWhenLow;
        settings.LowLevelThreshold = dto.LowLevelThreshold;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("request-password-change")]
    public IActionResult RequestPasswordChange()
    {
        return Ok(new
        {
            message = "Password change request received."
        });
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim))
            return null;

        if (!Guid.TryParse(userIdClaim, out var userId))
            return null;

        return userId;
    }
}