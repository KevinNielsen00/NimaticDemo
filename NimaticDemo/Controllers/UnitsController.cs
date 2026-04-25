using Backend.Data;
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

        var existingUnit = await _db.Units.FirstOrDefaultAsync(u => u.MacAddress == mac);

        if (existingUnit != null)
            return BadRequest("A unit with this MAC address already exists.");

        Guid customerId;

        if (User.IsInRole(nameof(AccountRole.Customer)))
        {
            var currentCustomerId = GetCustomerId();

            if (currentCustomerId == null)
                return Unauthorized();

            customerId = currentCustomerId.Value;
        }
        else if (User.IsInRole(nameof(AccountRole.Dealer)))
        {
            if (dto.CustomerId == null)
                return BadRequest("CustomerId is required for dealer users.");

            var dealerId = GetDealerId();

            if (dealerId == null)
                return Unauthorized();

            var customerBelongsToDealer = await _db.Customers
                .AnyAsync(c => c.Id == dto.CustomerId.Value && c.DealerId == dealerId.Value);

            if (!customerBelongsToDealer)
                return Forbid();

            customerId = dto.CustomerId.Value;
        }
        else if (User.IsInRole(nameof(AccountRole.Admin)))
        {
            if (dto.CustomerId == null)
                return BadRequest("CustomerId is required for admin users.");

            var customerExists = await _db.Customers
                .AnyAsync(c => c.Id == dto.CustomerId.Value);

            if (!customerExists)
                return BadRequest("Customer not found.");

            customerId = dto.CustomerId.Value;
        }
        else
        {
            return Unauthorized();
        }

        var unit = new Unit
        {
            CustomerId = customerId,
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

    private IQueryable<Unit>? GetAccessibleUnitsQuery()
    {
        var query = _db.Units
            .Include(u => u.Customer)
            .AsQueryable();

        if (User.IsInRole(nameof(AccountRole.Admin)))
        {
            return query;
        }

        if (User.IsInRole(nameof(AccountRole.Dealer)))
        {
            var dealerId = GetDealerId();

            if (dealerId == null)
                return null;

            return query.Where(u => u.Customer != null && u.Customer.DealerId == dealerId.Value);
        }

        if (User.IsInRole(nameof(AccountRole.Customer)))
        {
            var customerId = GetCustomerId();

            if (customerId == null)
                return null;

            return query.Where(u => u.CustomerId == customerId.Value);
        }

        return null;
    }

    private Guid? GetDealerId()
    {
        var dealerIdClaim = User.FindFirst("dealerId")?.Value;

        if (!Guid.TryParse(dealerIdClaim, out var dealerId))
            return null;

        return dealerId;
    }

    private Guid? GetCustomerId()
    {
        var customerIdClaim = User.FindFirst("customerId")?.Value;

        if (!Guid.TryParse(customerIdClaim, out var customerId))
            return null;

        return customerId;
    }
}