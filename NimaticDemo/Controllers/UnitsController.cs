using Backend.Data;
using Backend.DTOs;
using Backend.DTOs.Measurements;
using Backend.DTOs.Units;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var units = await _db.Units
            .Where(u => u.AccountId == userId.Value)
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
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var unit = await _db.Units
            .Where(u => u.Id == unitId && u.AccountId == userId.Value)
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
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var ownsUnit = await _db.Units
            .AnyAsync(u => u.Id == unitId && u.AccountId == userId.Value);

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
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var ownsUnit = await _db.Units
            .AnyAsync(u => u.Id == unitId && u.AccountId == userId.Value);

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
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(dto.MacAddress))
            return BadRequest("MacAddress is required.");

        var mac = dto.MacAddress.Trim().ToUpperInvariant();

        var existingUnit = await _db.Units
            .FirstOrDefaultAsync(u => u.MacAddress == mac);

        if (existingUnit != null)
            return BadRequest("A unit with this MAC address already exists.");

        var unit = new Unit
        {
            AccountId = userId.Value,
            MacAddress = mac,
            UnitName = string.IsNullOrWhiteSpace(dto.UnitName) ? null : dto.UnitName.Trim(),
            Location = string.IsNullOrWhiteSpace(dto.Location) ? null : dto.Location.Trim()
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