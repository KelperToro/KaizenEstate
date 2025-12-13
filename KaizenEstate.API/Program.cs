using KaizenEstate.API.Data;
using Microsoft.EntityFrameworkCore;
using KaizenEstate.API.Services;

var builder = WebApplication.CreateBuilder(args);

// === НАСТРОЙКА СЕРВИСОВ ===

// 1. Добавляем контроллеры
builder.Services.AddControllers();
builder.Services.AddScoped<IObjectStorageService, MinioService>();

// 2. Добавляем Swagger (теперь, после установки пакета, ошибки исчезнут)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3. Подключение к БД
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// === НАСТРОЙКА PIPELINE ===

// 4. Включаем Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();