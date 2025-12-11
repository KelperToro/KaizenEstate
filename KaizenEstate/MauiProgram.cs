using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Components.Authorization;
using KaizenEstate.Services;


namespace KaizenEstate
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
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

            // === 2. НАСТРОЙКИ ПОДКЛЮЧЕНИЯ ===
            builder.Services.AddScoped(sp => new HttpClient
            {
        
                BaseAddress = new Uri("https://localhost:7128/")
            });

            builder.Services.AddScoped<ClientApiService>();

            builder.Services.AddAuthorizationCore();

            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<AuthService>());

            return builder.Build();
        }
    }
}