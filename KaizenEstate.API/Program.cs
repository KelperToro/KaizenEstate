using KaizenEstate.API.Data;
using KaizenEstate.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// === 1. НАСТРОЙКИ ===
builder.Services.AddControllers();
builder.Services.AddScoped<IObjectStorageService, MinioService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Подключение БД
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// === 2. НАСТРОЙКА АВТОРИЗАЦИИ (JWT) ===
// Читаем ключ из appsettings.json
var jwtKey = builder.Configuration["AppSettings:Token"];

// Если ключа нет — используем запасной (но лучше, чтобы он был в конфиге!)
if (string.IsNullOrEmpty(jwtKey))
{
    jwtKey = "EtoOchenDlinniySecretniyKeyKotoriyNiktoNeUgadaet_Minimum64SimvolaDlyaSHA512";
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true
        };
    });

var app = builder.Build();

// === 3. PIPELINE ===
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Автомиграция
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        Console.WriteLine("--> База данных готова!");
    }
    catch (Exception ex)
    {
        Console.WriteLine("--> Ошибка БД: " + ex.Message);
    }
}

app.UseHttpsRedirection();

app.UseAuthentication(); // <-- ВАЖНО: Сначала проверка пропуска
app.UseAuthorization();  // <-- Потом проверка прав

app.MapControllers();

app.Run();