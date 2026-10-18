using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;

namespace Frontend.Services;

public class JwtAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;

    public JwtAuthStateProvider(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");
        if (string.IsNullOrWhiteSpace(token))
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        try
        {
            var parts = token.Split('.');
            if (parts.Length < 2) return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

            var payload = parts[1];
            payload = payload.Replace('-', '+').Replace('_', '/');
            switch (payload.Length % 4) { case 2: payload += "=="; break; case 3: payload += "="; break; }
            var jsonBytes = Convert.FromBase64String(payload);
            var json = Encoding.UTF8.GetString(jsonBytes);
            using var doc = JsonDocument.Parse(json);

            var claims = new List<Claim>();
            foreach (var p in doc.RootElement.EnumerateObject())
            {
                var name = p.Name;
                if (p.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var v in p.Value.EnumerateArray())
                        claims.Add(new Claim(name, v.ToString()));
                }
                else
                {
                    claims.Add(new Claim(name, p.Value.ToString() ?? ""));
                }
            }

            // Normalize typical role claim names
            var roleClaims = claims.Where(c => c.Type.Contains("role", StringComparison.OrdinalIgnoreCase)).ToList();
            if (!roleClaims.Any())
            {
                // fallback: claim type "role" may not exist
                if (doc.RootElement.TryGetProperty("role", out var r) && r.ValueKind == JsonValueKind.String)
                    claims.Add(new Claim(ClaimTypes.Role, r.GetString()!));
            }

            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);
            return new AuthenticationState(user);
        }
        catch
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    public void NotifyAuthenticationStateChanged() =>
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
}