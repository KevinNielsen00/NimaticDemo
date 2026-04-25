namespace Backend.Models;

public class Customer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;

    public Guid DealerId { get; set; }
    public Dealer? Dealer { get; set; }

    public List<Account> Accounts { get; set; } = new();
    public List<Unit> Units { get; set; } = new();
}