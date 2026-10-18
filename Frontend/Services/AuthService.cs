using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Blazored.LocalStorage;
using Frontend.Models;

namespace Frontend.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly ApiService _apiService;
    private readonly ILocalStorageService _localStorage;

    private const string TOKEN_KEY = "authToken";
    private const string ROLE_KEY = "authRole";

    public AuthService(HttpClient httpClient, ApiService apiService, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _apiService = apiService;
        _localStorage = localStorage;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Login failed: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

        if (result is not null && !string.IsNullOrWhiteSpace(result.Token))
        {
            await _localStorage.SetItemAsync(TOKEN_KEY, result.Token);
            if (!string.IsNullOrWhiteSpace(result.Role))
                await _localStorage.SetItemAsync(ROLE_KEY, result.Role);
            _apiService.SetToken(result.Token);
        }

        return result;
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _localStorage.GetItemAsync<string>(TOKEN_KEY);
    }

    public async Task<string?> GetRoleAsync()
    {
        // Try stored role first
        var stored = await _localStorage.GetItemAsync<string>(ROLE_KEY);
        if (!string.IsNullOrWhiteSpace(stored))
            return stored;

        // Fallback: decode JWT
        var token = await GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
            return null;

        token = token.Trim('"');
        var parts = token.Split('.');
        if (parts.Length < 2)
            return null;

        var payload = parts[1].Replace('-', '+').Replace('_', '/');
        switch (payload.Length % 4)
        {
            case 2: payload += "=="; break;
            case 3: payload += "="; break;
        }

        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("role", out var r) && r.ValueKind == JsonValueKind.String)
                return r.GetString();

            const string roleUri = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
            if (root.TryGetProperty(roleUri, out var r2) && r2.ValueKind == JsonValueKind.String)
                return r2.GetString();

            // fallback: any property name containing "role"
            foreach (var p in root.EnumerateObject())
            {
                if (p.Name.IndexOf("role", StringComparison.OrdinalIgnoreCase) >= 0 &&
                    p.Value.ValueKind == JsonValueKind.String)
                {
                    return p.Value.GetString();
                }
            }
        }
        catch
        {
            // ignore parse errors
        }

        return null;
    }

    public async Task<bool> IsLoggedInAsync()
    {
        var token = await _localStorage.GetItemAsync<string>(TOKEN_KEY);
        return !string.IsNullOrWhiteSpace(token);
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync(TOKEN_KEY);
        await _localStorage.RemoveItemAsync(ROLE_KEY);
        _apiService.SetToken(null);
    }

    public async Task ChangePasswordAsync(ChangePasswordDto dto)
    {
        var token = await GetTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
            _apiService.SetToken(token);

        var response = await _httpClient.PutAsJsonAsync("api/auth/change-password", dto);
        response.EnsureSuccessStatusCode();
    }
}