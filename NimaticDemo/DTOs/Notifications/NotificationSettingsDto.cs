namespace Backend.DTOs.Notifications;

public class NotificationSettingsDto
{
    public bool Enabled { get; set; }
    public int LowLevelThreshold { get; set; }
    public string? DealerEmail { get; set; }
    public string? ExtraCustomerEmail { get; set; }
}