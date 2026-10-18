using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace Frontend.Services;

public class JwtAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly IJSRuntime _jsRuntime;

    public JwtAuthStateProvider(ILocalStorageService localStorage, IJSRuntime jsRuntime)
    {
        _localStorage = localStorage;
        _jsRuntime = jsRuntime;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        string? token = null;
        try
        {
            token = await _localStorage.GetItemAsync<string>("authToken");
        }
        catch (JSException) // JS interop unavailable during prerender
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
        catch
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        if (string.IsNullOrWhiteSpace(token))
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        var principal = BuildClaimsPrincipalFromJwt(token);
        return new AuthenticationState(principal);
    }

    // Call this from client-only lifecycle (OnAfterRenderAsync) to refresh auth state after prerender
    public async Task NotifyAuthenticationStateFromLocalStorageAsync()
    {
        string? token = null;
        try
        {
            token = await _localStorage.GetItemAsync<string>("authToken");
        }
        catch
        {
            token = null;
        }

        ClaimsPrincipal principal = string.IsNullOrWhiteSpace(token)
            ? new ClaimsPrincipal(new ClaimsIdentity())
            : BuildClaimsPrincipalFromJwt(token);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
    }

    private ClaimsPrincipal BuildClaimsPrincipalFromJwt(string token)
    {
        try
        {
            token = token.Trim('"');
            var parts = token.Split('.');
            if (parts.Length < 2)
                return new ClaimsPrincipal(new ClaimsIdentity());

            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }

            var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            using var doc = JsonDocument.Parse(json);

            var claims = new List<Claim>();
            foreach (var p in doc.RootElement.EnumerateObject())
            {
                if (p.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var v in p.Value.EnumerateArray())
                        claims.Add(new Claim(p.Name, v.ToString()));
                }
                else
                {
                    claims.Add(new Claim(p.Name, p.Value.ToString() ?? ""));
                }
            }

            // Normalize role claim(s) to ClaimTypes.Role
            var roleClaims = new List<Claim>();
            foreach (var c in claims.Where(c => c.Type.IndexOf("role", StringComparison.OrdinalIgnoreCase) >= 0))
                roleClaims.Add(new Claim(ClaimTypes.Role, c.Value));

            var otherClaims = claims.Where(c => c.Type.IndexOf("role", StringComparison.OrdinalIgnoreCase) < 0)
                                    .Select(c => new Claim(c.Type, c.Value));

            var identity = new ClaimsIdentity(otherClaims.Concat(roleClaims), "jwt");
            return new ClaimsPrincipal(identity);
        }
        catch
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }
    }
}