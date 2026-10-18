using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace Frontend.Services;

public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
	private readonly ILocalStorageService _localStorage;
	private readonly ApiService _apiService;
	private const string TOKEN_KEY = "authToken";

	public JwtAuthenticationStateProvider(ILocalStorageService localStorage, ApiService apiService)
	{
		_localStorage = localStorage;
		_apiService = apiService;
	}

	public override async Task<AuthenticationState> GetAuthenticationStateAsync()
	{
		var token = await _localStorage.GetItemAsync<string>(TOKEN_KEY);
		if (string.IsNullOrWhiteSpace(token))
			return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

		_apiService.SetToken(token);
		var claims = ParseClaimsFromJwt(token);
		var identity = new ClaimsIdentity(claims, "jwt");
		var user = new ClaimsPrincipal(identity);
		return new AuthenticationState(user);
	}

	public void NotifyUserAuthentication(string token)
	{
		if (string.IsNullOrWhiteSpace(token))
		{
			NotifyUserLogout();
			return;
		}

		_apiService.SetToken(token);
		var claims = ParseClaimsFromJwt(token);
		var identity = new ClaimsIdentity(claims, "jwt");
		var user = new ClaimsPrincipal(identity);
		NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
	}

	public void NotifyUserLogout()
	{
		_apiService.SetToken(null);
		var anon = new ClaimsPrincipal(new ClaimsIdentity());
		NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anon)));
	}

	private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
	{
		jwt = jwt.Trim('"');
		var handler = new JwtSecurityTokenHandler();
		var token = handler.ReadJwtToken(jwt);
		var claims = token.Claims.ToList();

		// Ensure role claims are available as ClaimTypes.Role
		var roleClaimTypes = new[] { "role", "roles", "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" };
		var roles = token.Claims.Where(c => roleClaimTypes.Contains(c.Type, StringComparer.OrdinalIgnoreCase)).ToList();
		foreach (var r in roles)
		{
			claims.Add(new Claim(ClaimTypes.Role, r.Value));
		}

		return claims;
	}
}