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
        try
        {
            var dealer = new Dealer { Name = dto.Name };
            _db.Add(dealer);
            await _db.SaveChangesAsync();

            return Ok(new { Id = dealer.Id, Name = dealer.Name });
        }
        catch (Exception ex)
        {

            return Problem(title: "Could not create dealer", detail: ex.Message);
        }
    }

    [HttpGet("dealers")]
    public async Task<IActionResult> GetDealers()
    {
        var dealers = await _db.Dealers
            .Select(d => new { d.Id, d.Name })
            .ToListAsync();
        return Ok(dealers);
    }

    [HttpPost("customers")]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerDto dto)
    {
        try
        {
            var dealer = await _db.Dealers.FindAsync(dto.DealerId);
            if (dealer == null) return BadRequest("Dealer not found.");

            var customer = new Customer { Name = dto.Name, DealerId = dealer.Id };
            _db.Add(customer);
            await _db.SaveChangesAsync();

            return Ok(new { Id = customer.Id, Name = customer.Name, DealerId = customer.DealerId });
        }
        catch (Exception ex)
        {
            return Problem(title: "Could not create customer", detail: ex.Message);
        }
    }

    [HttpGet("customers")]
    public async Task<IActionResult> GetCustomers([FromQuery] Guid? dealerId = null)
    {
        var q = _db.Customers.AsQueryable();
        if (dealerId.HasValue) q = q.Where(c => c.DealerId == dealerId.Value);

        var customers = await q.Select(c => new { c.Id, c.Name, c.DealerId }).ToListAsync();
        return Ok(customers);
    }

    [HttpPost("accounts")]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDto dto)
    {
        try
        {
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
                PasswordResetRequired = true
            };

            _db.Add(account);
            await _db.SaveChangesAsync();

            return Ok(new { Id = account.Id, Email = account.Email, Role = account.Role.ToString(), PasswordResetRequired = account.PasswordResetRequired });
        }
        catch (Exception ex)
        {
            return Problem(title: "Could not create account", detail: ex.Message);
        }
    }

    [HttpGet("accounts")]
    public async Task<IActionResult> GetAccounts([FromQuery] Guid? dealerId = null, [FromQuery] Guid? customerId = null)
    {
        var q = _db.Accounts.AsQueryable();

        if (dealerId.HasValue)
            q = q.Where(a => a.DealerId == dealerId.Value);

        if (customerId.HasValue)
            q = q.Where(a => a.CustomerId == customerId.Value);

        var accounts = await q.Select(a => new
        {
            a.Id,
            a.Email,
            a.Name,
            a.Role,
            a.DealerId,
            a.CustomerId
        }).ToListAsync();

        return Ok(accounts);
    }

    [HttpPost("units/assign")]
    public async Task<IActionResult> AssignUnit([FromBody] AssignUnitDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.MacAddress))
            return BadRequest("MacAddress is required.");

        var account = await _db.Accounts.FindAsync(dto.AccountId);
        if (account == null) return BadRequest("Account not found.");

        var mac = dto.MacAddress.Trim().ToUpperInvariant();

        // Do NOT create unit here — require device registration first
        var unit = await _db.Units.FirstOrDefaultAsync(u => u.MacAddress.ToUpper() == mac);
        if (unit == null)
            return NotFound("Unit not found. Register the device before assigning it.");

        if (unit.AccountId == account.Id)
            return Conflict("Unit is already assigned to this account.");

        // assign/reassign to the requested account
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