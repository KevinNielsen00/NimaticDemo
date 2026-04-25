using System.Text.Json.Serialization;

namespace Backend.Models;

public class MqttPayload
{
    [JsonPropertyName("L")]
    public int L { get; set; }

    [JsonPropertyName("C")]
    public int C { get; set; }

    [JsonPropertyName("P")]
    public decimal P { get; set; }

    [JsonPropertyName("T")]
    public decimal T { get; set; }

    [JsonPropertyName("D")]
    public string D { get; set; } = string.Empty;
}