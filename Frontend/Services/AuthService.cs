using System.Net.Http.Json;
using Blazored.LocalStorage;
using Frontend.Models;

namespace Frontend.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly ApiService _apiService;
    private readonly ILocalStorageService _localStorage;

    private const string TOKEN_KEY = "authToken";

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
            _apiService.SetToken(result.Token);
        }

        return result;
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _localStorage.GetItemAsync<string>(TOKEN_KEY);
    }

    public async Task<bool> IsLoggedInAsync()
    {
        var token = await _localStorage.GetItemAsync<string>(TOKEN_KEY);
        return !string.IsNullOrWhiteSpace(token);
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync(TOKEN_KEY);
        _apiService.SetToken(null);
    }

    public async Task ChangePasswordAsync(ChangePasswordDto dto)
    {
        var token = await GetTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
        {
            _apiService.SetToken(token);
        }

        var response = await _httpClient.PutAsJsonAsync("api/auth/change-password", dto);
        response.EnsureSuccessStatusCode();

        // Clear local marker if needed: after changing password backend clears PasswordResetRequired.
    }
}