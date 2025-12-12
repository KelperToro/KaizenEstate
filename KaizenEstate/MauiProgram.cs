using Microsoft.Extensions.Logging;
using KaizenEstate.Services;
using Microsoft.AspNetCore.Components.Authorization;
using KaizenEstate.Shared.Models;
using Microsoft.Maui.Hosting;           // <--- Чинит ошибку "MauiApp не найден"
using Microsoft.Maui.Controls.Hosting;  // <--- Чинит ошибку "MauiApp не найден"
using System.Net.Http;                  // <--- Чинит HttpClient

namespace KaizenEstate
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            // ВНИМАНИЕ: Тут должно быть <App>, а не <EstateApplication>!
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
#endif

            // Настройки клиента (твой порт 7128)
            builder.Services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7128/")
            });

            builder.Services.AddScoped<ClientApiService>();

            // Авторизация
            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<AuthService>());

            return builder.Build();
        }
    }
}