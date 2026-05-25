using Backend.Data;
using Backend.DTOs.Measurements;
using Backend.DTOs.Units;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UnitsController : ControllerBase
{
    private readonly AppDbContext _db;

    public UnitsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetUnits()
    {
        var query = GetAccessibleUnitsQuery();

        if (query == null)
            return Unauthorized();

        var units = await query
            .Select(u => new UnitDto
            {
                Id = u.Id,
                MacAddress = u.MacAddress,
                UnitName = u.UnitName,
                Location = u.Location
            })
            .ToListAsync();

        return Ok(units);
    }

    [HttpGet("{unitId:guid}")]
    public async Task<IActionResult> GetUnitById(Guid unitId)
    {
        var query = GetAccessibleUnitsQuery();

        if (query == null)
            return Unauthorized();

        var unit = await query
            .Where(u => u.Id == unitId)
            .Select(u => new UnitDto
            {
                Id = u.Id,
                MacAddress = u.MacAddress,
                UnitName = u.UnitName,
                Location = u.Location
            })
            .FirstOrDefaultAsync();

        if (unit == null)
            return NotFound("Unit not found.");

        return Ok(unit);
    }

    [HttpGet("{unitId:guid}/latest")]
    public async Task<IActionResult> GetLatestMeasurement(Guid unitId)
    {
        var query = GetAccessibleUnitsQuery();

        if (query == null)
            return Unauthorized();

        var ownsUnit = await query.AnyAsync(u => u.Id == unitId);

        if (!ownsUnit)
            return NotFound("Unit not found.");

        var measurement = await _db.Measurements
            .Where(m => m.UnitId == unitId)
            .OrderByDescending(m => m.MeasuredAt)
            .Select(m => new MeasurementDto
            {
                Depth = m.Depth,
                Restarts = m.Restarts,
                Battery = m.Battery,
                Temperature = m.Temperature,
                MeasuredAt = m.MeasuredAt
            })
            .FirstOrDefaultAsync();

        if (measurement == null)
            return NotFound("No measurements found for this unit.");

        return Ok(measurement);
    }

    [HttpGet("{unitId:guid}/measurements")]
    public async Task<IActionResult> GetMeasurements(Guid unitId)
    {
        var query = GetAccessibleUnitsQuery();

        if (query == null)
            return Unauthorized();

        var ownsUnit = await query.AnyAsync(u => u.Id == unitId);

        if (!ownsUnit)
            return NotFound("Unit not found.");

        var measurements = await _db.Measurements
            .Where(m => m.UnitId == unitId)
            .OrderByDescending(m => m.MeasuredAt)
            .Take(100)
            .Select(m => new MeasurementDto
            {
                Depth = m.Depth,
                Restarts = m.Restarts,
                Battery = m.Battery,
                Temperature = m.Temperature,
                MeasuredAt = m.MeasuredAt
            })
            .ToListAsync();

        return Ok(measurements);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUnit([FromBody] CreateUnitDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.MacAddress))
            return BadRequest("MacAddress is required.");

        var mac = dto.MacAddress.Trim().ToUpperInvariant();

        var existingUnit = await _db.Units
            .FirstOrDefaultAsync(u => u.MacAddress == mac);

        if (existingUnit != null)
            return BadRequest("A unit with this MAC address already exists.");

        var accountId = GetAccountId();

        if (accountId == null)
            return Unauthorized();

        if (!User.IsInRole(nameof(AccountRole.Admin)))
        {
            var accountExists = await _db.Accounts
                .AnyAsync(a => a.Id == accountId.Value);

            if (!accountExists)
                return Unauthorized();
        }

        var unit = new Unit
        {
            AccountId = accountId.Value,
            MacAddress = mac,
            UnitName = string.IsNullOrWhiteSpace(dto.UnitName) ? null : dto.UnitName.Trim(),
            Location = string.IsNullOrWhiteSpace(dto.Location) ? null : dto.Location.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _db.Units.Add(unit);
        await _db.SaveChangesAsync();

        var result = new UnitDto
        {
            Id = unit.Id,
            MacAddress = unit.MacAddress,
            UnitName = unit.UnitName,
            Location = unit.Location
        };

        return CreatedAtAction(nameof(GetUnitById), new { unitId = unit.Id }, result);
    }

    [HttpPost("claim")]
    public async Task<IActionResult> ClaimUnit([FromBody] ClaimUnitDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.MacAddress))
            return BadRequest("MacAddress is required.");

        var mac = dto.MacAddress.Trim().ToUpperInvariant();

        var accountId = GetAccountId();

        if (accountId == null)
            return Unauthorized();

        var unit = await _db.Units
            .FirstOrDefaultAsync(u => u.MacAddress == mac);

        if (unit == null)
            return NotFound("Unit not found.");

        unit.AccountId = accountId.Value;

        await _db.SaveChangesAsync();

        return Ok(new UnitDto
        {
            Id = unit.Id,
            MacAddress = unit.MacAddress,
            UnitName = unit.UnitName,
            Location = unit.Location
        });
    }

    private IQueryable<Unit>? GetAccessibleUnitsQuery()
    {
        var query = _db.Units
            .Include(u => u.Account)
            .AsQueryable();

        if (User.IsInRole(nameof(AccountRole.Admin)))
            return query;

        if (User.IsInRole(nameof(AccountRole.Dealer)))
        {
            var dealerId = GetDealerId();

            if (dealerId == null)
                return null;

            return query.Where(u =>
                u.Account != null &&
                u.Account.DealerId == dealerId.Value);
        }

        if (User.IsInRole(nameof(AccountRole.Customer)))
        {
            var accountId = GetAccountId();

            if (accountId == null)
                return null;

            return query.Where(u => u.AccountId == accountId.Value);
        }

        return null;
    }

    private Guid? GetAccountId()
    {
        var accountIdClaim =
            User.FindFirst("accountId")?.Value ??
            User.FindFirst("sub")?.Value ??
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(accountIdClaim, out var accountId))
            return null;

        return accountId;
    }

    private Guid? GetDealerId()
    {
        var dealerIdClaim = User.FindFirst("dealerId")?.Value;

        if (!Guid.TryParse(dealerIdClaim, out var dealerId))
            return null;

        return dealerId;
    }
}