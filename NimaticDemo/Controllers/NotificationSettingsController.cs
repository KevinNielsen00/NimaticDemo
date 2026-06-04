using System.Security.Claims;
using Backend.Data;
using Backend.DTOs.Notifications;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[Authorize]
[ApiController]
[Route("api/notification-settings")]
public class NotificationSettingsController : ControllerBase
{
    private readonly AppDbContext _db;

    public NotificationSettingsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<NotificationSettingsDto>> Get()
    {
        var accountId = GetAccountId();

        if (accountId is null)
            return Unauthorized();

        var settings = await GetOrCreateSettings(accountId.Value);

        return Ok(new NotificationSettingsDto
        {
            Enabled = settings.Enabled,
            LowLevelThreshold = settings.LowLevelThreshold,
            DealerEmail = settings.DealerEmail,
            ExtraCustomerEmail = settings.ExtraCustomerEmail
        });
    }

    [HttpPut]
    public async Task<IActionResult> Update(NotificationSettingsDto dto)
    {
        var accountId = GetAccountId();

        if (accountId is null)
            return Unauthorized();

        var settings = await GetOrCreateSettings(accountId.Value);

        settings.Enabled = dto.Enabled;
        settings.LowLevelThreshold = Math.Clamp(dto.LowLevelThreshold, 1, 100);
        settings.ExtraCustomerEmail = string.IsNullOrWhiteSpace(dto.ExtraCustomerEmail)
            ? null
            : dto.ExtraCustomerEmail.Trim();

        if (User.IsInRole(nameof(AccountRole.Admin)) ||
            User.IsInRole(nameof(AccountRole.Dealer)))
        {
            settings.DealerEmail = string.IsNullOrWhiteSpace(dto.DealerEmail)
                ? null
                : dto.DealerEmail.Trim();
        }

        await _db.SaveChangesAsync();

        return NoContent();
    }

    private async Task<NotificationSettings> GetOrCreateSettings(Guid accountId)
    {
        var settings = await _db.NotificationSettings
            .FirstOrDefaultAsync(s => s.AccountId == accountId);

        if (settings is not null)
            return settings;

        settings = new NotificationSettings
        {
            AccountId = accountId,
            Enabled = true,
            LowLevelThreshold = 25
        };

        _db.NotificationSettings.Add(settings);
        await _db.SaveChangesAsync();

        return settings;
    }

    private Guid? GetAccountId()
    {
        var claim =
            User.FindFirst("accountId")?.Value ??
            User.FindFirst("sub")?.Value ??
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(claim, out var id) ? id : null;
    }
}