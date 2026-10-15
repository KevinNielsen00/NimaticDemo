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

    // Helper: hent dealerId fra token
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

        var customer = new Customer
        {
            Name = dto.Name,
            DealerId = dealerId.Value
        };

        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();
        return Ok(customer);
    }

    // Dealer kan oprette kunde-konti under sin dealer
    [HttpPost("accounts")]
    public async Task<IActionResult> CreateCustomerAccount([FromBody] CreateAccountDto dto)
    {
        var dealerId = GetDealerId();
        if (dealerId == null) return Forbid();

        var email = dto.Email.Trim().ToLower();
        if (await _db.Accounts.AnyAsync(a => a.Email == email))
            return BadRequest("Email already exists.");

        // Hvis dto.CustomerId er sat, verificer at customer tilhører denne dealer
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
            Role = dto.Role == AccountRole.Admin ? AccountRole.Customer : dto.Role, // dealer bør ikke kunne oprette Admin
            DealerId = dealerId,
            CustomerId = dto.CustomerId,
            PasswordResetRequired = true
        };

        _db.Accounts.Add(account);
        await _db.SaveChangesAsync();
        return Ok(new { account.Id, account.Email, account.Role, account.PasswordResetRequired });
    }

    // Dealer kan tilknytte en unit til en account - kun hvis target account tilhører samme dealer (som direkte account.DealerId eller via customer)
    [HttpPost("units/assign")]
    public async Task<IActionResult> AssignUnit([FromBody] AssignUnitDto dto)
    {
        var dealerId = GetDealerId();
        if (dealerId == null) return Forbid();

        var account = await _db.Accounts.FindAsync(dto.AccountId);
        if (account == null) return BadRequest("Account not found.");

        // Check at account tilhører dealer (enten direkte eller via customer)
        var belongs = (account.DealerId.HasValue && account.DealerId.Value == dealerId.Value);
        if (!belongs && account.CustomerId.HasValue)
        {
            var customer = await _db.Customers.FindAsync(account.CustomerId.Value);
            belongs = customer != null && customer.DealerId == dealerId.Value;
        }

        if (!belongs) return Forbid();

        var mac = dto.MacAddress.Trim();
        var unit = await _db.Units.FirstOrDefaultAsync(u => u.MacAddress.ToLower() == mac.ToLower());

        if (unit == null)
        {
            unit = new Unit
            {
                MacAddress = mac,
                AccountId = account.Id,
                UnitName = dto.UnitName,
                Location = dto.Location
            };
            _db.Units.Add(unit);
        }
        else
        {
            unit.AccountId = account.Id;
            if (!string.IsNullOrWhiteSpace(dto.UnitName)) unit.UnitName = dto.UnitName;
            if (!string.IsNullOrWhiteSpace(dto.Location)) unit.Location = dto.Location;
            _db.Units.Update(unit);
        }

        await _db.SaveChangesAsync();
        return Ok(unit);
    }
}