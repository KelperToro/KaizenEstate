using System.Net.Http.Json;
using System.Security.Claims;
using KaizenEstate.Shared.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Maui.Storage;

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

        // === НОВЫЙ МЕТОД: ВОССТАНОВИТЬ СЕССИЮ ПРИ ЗАПУСКЕ ===
        public async Task InitializeAsync()
        {
            try
            {
                var token = await SecureStorage.GetAsync("auth_token");
                var email = await SecureStorage.GetAsync("user_email");
                var role = await SecureStorage.GetAsync("user_role"); 
                var name = await SecureStorage.GetAsync("user_name");

                if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(email))
                {
                    // Восстанавливаем пользователя
                    var identity = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, name ?? "User"),
                        new Claim(ClaimTypes.Email, email),
                        new Claim(ClaimTypes.Role, role ?? "User"), 
                    }, "CustomAuth");

                    _currentUser = new ClaimsPrincipal(identity);
                    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
                }
            }
            catch
            {
                
            }
        }

        public async Task<string?> LoginAsync(LoginModel model)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", model);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (result != null)
                    {
                        // 1. Сохраняем ВСЁ (Токен + Инфо)
                        await SecureStorage.SetAsync("auth_token", result.Token);
                        await SecureStorage.SetAsync("user_email", result.User.Email);
                        await SecureStorage.SetAsync("user_role", result.User.Role);
                        await SecureStorage.SetAsync("user_name", result.User.FullName);

                        var identity = new ClaimsIdentity(new[]
                        {
                            new Claim(ClaimTypes.Name, result.User.FullName),
                            new Claim(ClaimTypes.Email, result.User.Email),
                            new Claim(ClaimTypes.Role, result.User.Role),
                            new Claim(ClaimTypes.NameIdentifier, result.User.Id.ToString())
                        }, "CustomAuth");

                        _currentUser = new ClaimsPrincipal(identity);
                        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
                        return null;
                    }
                }
                return await response.Content.ReadAsStringAsync();
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
                return await LoginAsync(new LoginModel { Email = model.Email, Password = model.Password });
            }
            return await response.Content.ReadAsStringAsync();
        }

        public void Logout()
        {
            _currentUser = new(new ClaimsIdentity());
     
            SecureStorage.Remove("auth_token");
            SecureStorage.Remove("user_email");
            SecureStorage.Remove("user_role");
            SecureStorage.Remove("user_name");
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        private class LoginResponse
        {
            public string Token { get; set; }
            public User User { get; set; }
        }
    }
}