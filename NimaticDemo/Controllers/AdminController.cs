using Backend.Data;
using Backend.DTOs.Admin;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost("dealers")]
    public async Task<IActionResult> CreateDealer([FromBody] CreateDealerDto dto)
    {
        var dealer = new Dealer { Name = dto.Name };
        _db.Add(dealer);
        await _db.SaveChangesAsync();
        return Ok(dealer);
    }

    [HttpPost("customers")]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerDto dto)
    {
        var dealer = await _db.Dealers.FindAsync(dto.DealerId);
        if (dealer == null) return BadRequest("Dealer not found.");

        var customer = new Customer { Name = dto.Name, DealerId = dealer.Id };
        _db.Add(customer);
        await _db.SaveChangesAsync();
        return Ok(customer);
    }

    [HttpPost("accounts")]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDto dto)
    {
        // optional validation of unique email
        var email = dto.Email.Trim().ToLower();
        if (await _db.Accounts.AnyAsync(a => a.Email == email))
            return BadRequest("Email already exists.");

        var account = new Account
        {
            Name = dto.Name,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.TempPassword),
            Role = dto.Role,
            DealerId = dto.DealerId,
            CustomerId = dto.CustomerId,
            PasswordResetRequired = true // force change on first login
        };

        _db.Add(account);
        await _db.SaveChangesAsync();
        return Ok(new { account.Id, account.Email, account.Role, account.PasswordResetRequired });
    }

    [HttpPost("units/assign")]
    public async Task<IActionResult> AssignUnit([FromBody] AssignUnitDto dto)
    {
        // Validate target account exists
        var account = await _db.Accounts.FindAsync(dto.AccountId);
        if (account == null) return BadRequest("Account not found.");

        // Find existing Unit by MacAddress (case-insensitive)
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