using Backend.Data;
using Backend.DTOs.Auth;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokenService;

    public AuthController(AppDbContext db, TokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest("Name, email og password skal udfyldes.");
        }

        var email = dto.Email.Trim().ToLower();

        var existingUser = await _db.Accounts.FirstOrDefaultAsync(a => a.Email == email);
        if (existingUser != null)
            return BadRequest("En bruger med den email findes allerede.");

        var customerExists = await _db.Customers.AnyAsync(c => c.Id == dto.CustomerId);
        if (!customerExists)
            return BadRequest("CustomerId findes ikke.");

        var account = new Account
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = AccountRole.Customer,
            CustomerId = dto.CustomerId
        };

        _db.Accounts.Add(account);
        await _db.SaveChangesAsync();

        var token = _tokenService.CreateToken(account);

        return Ok(new AuthResponseDto
        {
            Token = token,
            Email = account.Email
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("Email og password skal udfyldes.");

        var email = dto.Email.Trim().ToLower();

        var account = await _db.Accounts.FirstOrDefaultAsync(a => a.Email == email);
        if (account == null)
            return Unauthorized("Forkert email eller password.");

        var passwordValid = BCrypt.Net.BCrypt.Verify(dto.Password, account.PasswordHash);
        if (!passwordValid)
            return Unauthorized("Forkert email eller password.");

        var token = _tokenService.CreateToken(account);

        return Ok(new AuthResponseDto
        {
            Token = token,
            Email = account.Email
        });
    }
}