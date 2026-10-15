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

    public async Task<DealerDto?> CreateDealerAsync(CreateDealerDto dto)
    {
        var resp = await _httpClient.PostAsJsonAsync("api/admin/dealers", dto);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<DealerDto>();
    }

    public async Task<CustomerDto?> CreateCustomerAsync(CreateCustomerDto dto, bool asDealer = false)
    {
        var url = asDealer ? "api/dealer/customers" : "api/admin/customers";
        var resp = await _httpClient.PostAsJsonAsync(url, dto);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<CustomerDto>();
    }

    public async Task<AccountCreatedResponse?> CreateAccountAsync(CreateAccountDto dto, bool asDealer = false)
    {
        var url = asDealer ? "api/dealer/accounts" : "api/admin/accounts";
        var resp = await _httpClient.PostAsJsonAsync(url, dto);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<AccountCreatedResponse>();
    }

    public async Task<UnitDto?> AssignUnitAsync(AssignUnitDto dto, bool asDealer = false)
    {
        var url = asDealer ? "api/dealer/units/assign" : "api/admin/units/assign";
        var resp = await _httpClient.PostAsJsonAsync(url, dto);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<UnitDto>();
    }

    public async Task<List<DealerDto>> GetDealersAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<DealerDto>>("api/admin/dealers")
               ?? new List<DealerDto>();
    }

    public async Task<List<CustomerDto>> GetCustomersAsync(Guid? dealerId = null, bool asDealer = false)
    {
        if (asDealer)
        {
            return await _httpClient.GetFromJsonAsync<List<CustomerDto>>("api/dealer/customers")
                   ?? new List<CustomerDto>();
        }
        else
        {
            var url = dealerId.HasValue ? $"api/admin/customers?dealerId={dealerId}" : "api/admin/customers";
            return await _httpClient.GetFromJsonAsync<List<CustomerDto>>(url)
                   ?? new List<CustomerDto>();
        }
    }

    public async Task<List<AccountDto>> GetAccountsAsync(Guid? dealerId = null, Guid? customerId = null, bool asDealer = false)
    {
        if (asDealer)
        {
            return await _httpClient.GetFromJsonAsync<List<AccountDto>>("api/dealer/accounts")
                   ?? new List<AccountDto>();
        }
        else
        {
            var qs = "";
            if (dealerId.HasValue) qs += $"dealerId={dealerId.Value}";
            if (customerId.HasValue) qs += (qs == "" ? "" : "&") + $"customerId={customerId.Value}";
            var url = $"api/admin/accounts{(qs == "" ? "" : "?" + qs)}";
            return await _httpClient.GetFromJsonAsync<List<AccountDto>>(url)
                   ?? new List<AccountDto>();
        }
    }
}