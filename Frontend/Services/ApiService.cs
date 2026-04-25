using System.Net.Http.Headers;
using System.Net.Http.Json;
using Frontend.Models;

namespace Frontend.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public void SetToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
            return;
        }

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<List<UnitDto>> GetUnitsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<UnitDto>>("api/units")
               ?? new List<UnitDto>();
    }

    public async Task<List<MeasurementDto>> GetMeasurementsAsync(Guid unitId)
    {
        return await _httpClient.GetFromJsonAsync<List<MeasurementDto>>(
                   $"api/units/{unitId}/measurements")
               ?? new List<MeasurementDto>();
    }

    public async Task<SettingsDto?> GetSettingsAsync()
    {
        return await _httpClient.GetFromJsonAsync<SettingsDto>("api/settings");
    }

    public async Task UpdateSettingsAsync(SettingsDto settings)
    {
        var response = await _httpClient.PutAsJsonAsync("api/settings", settings);
        response.EnsureSuccessStatusCode();
    }

    public async Task RequestPasswordChangeAsync()
    {
        var response = await _httpClient.PostAsync("api/settings/request-password-change", null);
        response.EnsureSuccessStatusCode();
    }
}