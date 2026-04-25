using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public class Account
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public AccountRole Role { get; set; }

    public Guid? DealerId { get; set; }
    public Dealer? Dealer { get; set; }

    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }
}
