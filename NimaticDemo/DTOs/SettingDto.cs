namespace Backend.DTOs;

public class SettingsDto
{
    public bool DarkMode { get; set; }
    public bool AutoEmailWhenLow { get; set; }
    public int LowLevelThreshold { get; set; }
}