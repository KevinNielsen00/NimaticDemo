using Backend.Data;
using Backend.DTOs.Auth;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
            Email = account.Email,
            MustChangePassword = account.PasswordResetRequired
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
            Email = account.Email,
            MustChangePassword = account.PasswordResetRequired
        });
    }

    [Authorize]
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrWhiteSpace(sub) || !Guid.TryParse(sub, out var userId))
            return Unauthorized();

        var account = await _db.Accounts.FindAsync(userId);
        if (account == null)
            return Unauthorized();

        if (!account.PasswordResetRequired)
        {
            if (string.IsNullOrWhiteSpace(dto.CurrentPassword))
                return BadRequest("Current password required.");

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, account.PasswordHash))
                return Unauthorized("Current password incorrect.");
        }

        account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        account.PasswordResetRequired = false;

        await _db.SaveChangesAsync();
        return NoContent();
    }
}