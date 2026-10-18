using Backend.Data;
using Backend.DTOs.Admin;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Controllers;

[ApiController]
[Route("api/dealer")]
[Authorize(Roles = "Dealer")]
public class DealerController : ControllerBase
{
    private readonly AppDbContext _db;

    public DealerController(AppDbContext db)
    {
        _db = db;
    }

    private Guid? GetDealerId()
    {
        var claim = User.FindFirst("dealerId")?.Value;
        if (string.IsNullOrWhiteSpace(claim)) return null;
        return Guid.TryParse(claim, out var id) ? id : null;
    }

    [HttpPost("customers")]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerDto dto)
    {
        var dealerId = GetDealerId();
        if (dealerId == null) return Forbid();

        var customer = new Customer { Name = dto.Name, DealerId = dealerId.Value };
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();
        return Ok(customer);
    }

    [HttpGet("customers")]
    public async Task<IActionResult> GetCustomers()
    {
        var dealerId = GetDealerId();
        if (dealerId == null) return Forbid();

        var customers = await _db.Customers
            .Where(c => c.DealerId == dealerId.Value)
            .Select(c => new { c.Id, c.Name, c.DealerId })
            .ToListAsync();

        return Ok(customers);
    }

    [HttpPost("accounts")]
    public async Task<IActionResult> CreateCustomerAccount([FromBody] CreateAccountDto dto)
    {
        var dealerId = GetDealerId();
        if (dealerId == null) return Forbid();

        var email = dto.Email.Trim().ToLower();
        if (await _db.Accounts.AnyAsync(a => a.Email == email))
            return BadRequest("Email already exists.");

        if (dto.CustomerId.HasValue)
        {
            var cust = await _db.Customers.FindAsync(dto.CustomerId.Value);
            if (cust == null || cust.DealerId != dealerId.Value)
                return BadRequest("Invalid CustomerId for this dealer.");
        }

        var account = new Account
        {
            Name = dto.Name,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.TempPassword),
            Role = dto.Role == AccountRole.Admin ? AccountRole.Customer : dto.Role,
            DealerId = dealerId,
            CustomerId = dto.CustomerId,
            PasswordResetRequired = true
        };

        _db.Accounts.Add(account);
        await _db.SaveChangesAsync();
        return Ok(new { account.Id, account.Email, account.Role, account.PasswordResetRequired });
    }

    [HttpGet("accounts")]
    public async Task<IActionResult> GetAccounts()
    {
        var dealerId = GetDealerId();
        if (dealerId == null) return Forbid();

        var accounts = await _db.Accounts
            .Where(a => (a.DealerId.HasValue && a.DealerId.Value == dealerId.Value) ||
                        (a.CustomerId.HasValue && _db.Customers.Any(c => c.Id == a.CustomerId && c.DealerId == dealerId.Value)))
            .Select(a => new { a.Id, a.Email, a.Name, a.Role, a.DealerId, a.CustomerId })
            .ToListAsync();

        return Ok(accounts);
    }

    [HttpPost("units/assign")]
    public async Task<IActionResult> AssignUnit([FromBody] AssignUnitDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.MacAddress))
            return BadRequest("MacAddress is required.");

        var dealerId = GetDealerId();
        if (dealerId == null) return Forbid();

        var account = await _db.Accounts.FindAsync(dto.AccountId);
        if (account == null) return BadRequest("Account not found.");

        // ensure the account belongs to this dealer
        var accountBelongsToDealer = (account.DealerId.HasValue && account.DealerId == dealerId)
                                      || (account.CustomerId.HasValue && _db.Customers.Any(c => c.Id == account.CustomerId && c.DealerId == dealerId))
                                      || account.Id == account.Id && account.DealerId == dealerId; // defensive

        if (!accountBelongsToDealer)
            return Forbid();

        var mac = dto.MacAddress.Trim().ToUpperInvariant();

        // require unit exists
        var unit = await _db.Units.FirstOrDefaultAsync(u => u.MacAddress.ToUpper() == mac);
        if (unit == null)
            return NotFound("Unit not found. Register the device before assigning it.");

        if (unit.AccountId == account.Id)
            return Conflict("Unit is already assigned to this account.");

        // assign/reassign
        unit.AccountId = account.Id;
        unit.CustomerId = account.CustomerId;
        if (!string.IsNullOrWhiteSpace(dto.UnitName)) unit.UnitName = dto.UnitName.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Location)) unit.Location = dto.Location.Trim();
        unit.CreatedAt = unit.CreatedAt == default ? DateTime.UtcNow : unit.CreatedAt;

        _db.Units.Update(unit);
        await _db.SaveChangesAsync();

        var result = new Backend.DTOs.Units.UnitDto
        {
            Id = unit.Id,
            MacAddress = unit.MacAddress,
            UnitName = unit.UnitName,
            Location = unit.Location,
            AccountId = unit.AccountId,
            CustomerId = unit.CustomerId,
            CreatedAt = unit.CreatedAt
        };

        return Ok(result);
    }
}