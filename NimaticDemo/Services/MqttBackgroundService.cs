using Microsoft.EntityFrameworkCore;
using MQTTnet;
using Backend.Data;
using Backend.Models;
using System.Text;
using System.Text.Json;

namespace Backend.Services;

public class MqttBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MqttBackgroundService> _logger;
    private IMqttClient? _mqttClient;

    public MqttBackgroundService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<MqttBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new MqttClientFactory();
        _mqttClient = factory.CreateMqttClient();

        _mqttClient.ApplicationMessageReceivedAsync += async e =>
        {
            try
            {
                var payloadString = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                _logger.LogInformation("MQTT message received: {Payload}", payloadString);

                var payload = JsonSerializer.Deserialize<MqttPayload>(payloadString);

                if (payload is null || string.IsNullOrWhiteSpace(payload.D))
                {
                    _logger.LogWarning("Invalid payload received.");
                    return;
                }

                await SaveMeasurementAsync(payload, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing MQTT message.");
            }
        };

        var broker = _configuration["Mqtt:Broker"] ?? "localhost";
        var topic = _configuration["Mqtt:Topic"] ?? "iot/data";
        var username = _configuration["Mqtt:Username"];
        var password = _configuration["Mqtt:Password"];

        var optionsBuilder = new MqttClientOptionsBuilder()
            .WithTcpServer(broker);

        if (!string.IsNullOrWhiteSpace(username))
        {
            optionsBuilder.WithCredentials(username, password);
        }

        var options = optionsBuilder.Build();

        await _mqttClient.ConnectAsync(options, stoppingToken);
        _logger.LogInformation("Connected to MQTT broker.");

        await _mqttClient.SubscribeAsync(topic, cancellationToken: stoppingToken);
        _logger.LogInformation("Subscribed to topic: {Topic}", topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task SaveMeasurementAsync(MqttPayload payload, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var mac = payload.D.Trim().ToUpperInvariant();

        var unit = await db.Units
            .FirstOrDefaultAsync(u => u.MacAddress == mac, cancellationToken);

        if (unit is null)
        {
            var adminAccountIdConfig = _configuration["AdminAccountId"];

            if (string.IsNullOrWhiteSpace(adminAccountIdConfig) ||
                !Guid.TryParse(adminAccountIdConfig, out var adminAccountId))
            {
                _logger.LogWarning("Unit not found and AdminAccountId is missing/invalid. MAC: {Mac}", mac);
                return;
            }

            var adminAccountExists = await db.Accounts
                .AnyAsync(a => a.Id == adminAccountId, cancellationToken);

            if (!adminAccountExists)
            {
                _logger.LogWarning("AdminAccountId does not match any account. AdminAccountId: {AdminAccountId}", adminAccountId);
                return;
            }

            unit = new Unit
            {
                AccountId = adminAccountId,
                MacAddress = mac,
                UnitName = $"Unit-{mac}",
                CreatedAt = DateTime.UtcNow
            };

            db.Units.Add(unit);
            await db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("New unit auto-created and linked to admin. MAC: {Mac}", mac);
        }

        var measurement = new Measurement
        {
            UnitId = unit.Id,
            Depth = payload.L,
            Restarts = payload.C,
            Battery = payload.P,
            Temperature = payload.T,
            MeasuredAt = DateTime.UtcNow
        };

        db.Measurements.Add(measurement);
        await db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Measurement saved for unit {Mac}", mac);
    }
}