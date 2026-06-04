namespace Backend.Models;

public class NotificationSettings
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;

    public bool Enabled { get; set; } = true;

    public int LowLevelThreshold { get; set; } = 25;

    public string? DealerEmail { get; set; }

    public string? ExtraCustomerEmail { get; set; }

    public DateTime? LastEmailSentAt { get; set; }
}