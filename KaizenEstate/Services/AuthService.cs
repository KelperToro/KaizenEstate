using System.Net.Http.Json;
using System.Security.Claims;
using KaizenEstate.Shared.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace KaizenEstate.Services
{
    public class AuthService : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;

        private ClaimsPrincipal _currentUser = new(new ClaimsIdentity());

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(new AuthenticationState(_currentUser));
        }

        public async Task<string?> LoginAsync(LoginModel model)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", model);

                if (response.IsSuccessStatusCode)
                {
                    var user = await response.Content.ReadFromJsonAsync<User>();

                    var identity = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, user.FullName),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, user.Role),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                    }, "CustomAuth");

                    _currentUser = new ClaimsPrincipal(identity);

                    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
                    return null;
                }
                else
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                return "Ошибка подключения: " + ex.Message;
            }
        }

        public async Task<string?> RegisterAsync(RegisterModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", model);

            if (response.IsSuccessStatusCode)
            {
                return await LoginAsync(new LoginModel
                {
                    Email = model.Email,
                    Password = model.Password
                });
            }

            return await response.Content.ReadAsStringAsync();
        }

        public void Logout()
        {
            _currentUser = new(new ClaimsIdentity()); 
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}