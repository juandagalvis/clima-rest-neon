using Microsoft.EntityFrameworkCore;
using Npgsql;
using RestFulWebService.Data;
using RestFulWebService.Models;

var builder = WebApplication.CreateBuilder(args);

// Render define PORT para cada servicio. Escuchamos en todas las interfaces.
var renderPort = Environment.GetEnvironmentVariable("PORT");
if (int.TryParse(renderPort, out var port) && port > 0)
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// En Render se define DATABASE_URL con la cadena de Neon.
// En desarrollo local, si no existe DATABASE_URL, se usa SQLite.
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrWhiteSpace(databaseUrl))
{
    var postgresConnectionString = ConvertDatabaseUrl(databaseUrl);
    builder.Services.AddDbContext<WeatherDbContext>(options =>
        options.UseNpgsql(postgresConnectionString, npgsqlOptions =>
            npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null)));
}
else if (builder.Environment.IsDevelopment())
{
    var databasePath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "weather.db");
    Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);

    builder.Services.AddDbContext<WeatherDbContext>(options =>
        options.UseSqlite($"Data Source={databasePath}"));
}
else
{
    throw new InvalidOperationException(
        "Falta configurar DATABASE_URL. En Render, agrega la cadena de conexión de Neon como variable de entorno secreta.");
}

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();
app.MapOpenApi();

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    application = "Clima REST",
    database = string.IsNullOrWhiteSpace(databaseUrl) ? "SQLite (local)" : "PostgreSQL (Neon)",
    timestamp = DateTimeOffset.UtcNow
}));

// Crea las tablas automáticamente y agrega registros de ejemplo la primera vez.
// La base de datos de Neon debe estar vacía al primer despliegue.
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WeatherDbContext>();
    await db.Database.EnsureCreatedAsync();

    if (!await db.Forecasts.AnyAsync())
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        db.Forecasts.AddRange(
            new WeatherForecast { Date = today, TemperatureC = 24, Summary = "Soleado" },
            new WeatherForecast { Date = today.AddDays(1), TemperatureC = 21, Summary = "Parcialmente nublado" },
            new WeatherForecast { Date = today.AddDays(2), TemperatureC = 18, Summary = "Lluvioso" },
            new WeatherForecast { Date = today.AddDays(3), TemperatureC = 26, Summary = "Despejado" },
            new WeatherForecast { Date = today.AddDays(4), TemperatureC = 20, Summary = "Brisa" }
        );
        await db.SaveChangesAsync();
    }
}

app.Run();

// Neon entrega una URL postgresql://..., mientras Npgsql utiliza parámetros ADO.NET.
// Convertimos la URL sin imprimir ni guardar la contraseña en archivos del proyecto.
static string ConvertDatabaseUrl(string value)
{
    if (!value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
        !value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
    {
        // También permite una cadena Npgsql estándar: Host=...;Database=...;...
        return value;
    }

    if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || string.IsNullOrWhiteSpace(uri.Host))
    {
        throw new InvalidOperationException("DATABASE_URL no tiene un formato PostgreSQL válido. Copia la URL desde el botón Connect de Neon.");
    }

    var userInfo = uri.UserInfo.Split(':', 2);
    if (userInfo.Length != 2)
    {
        throw new InvalidOperationException("DATABASE_URL debe incluir el usuario y la contraseña generados por Neon.");
    }

    var databaseName = Uri.UnescapeDataString(uri.AbsolutePath.Trim('/'));
    if (string.IsNullOrWhiteSpace(databaseName))
    {
        throw new InvalidOperationException("DATABASE_URL no incluye el nombre de la base de datos.");
    }

    var connection = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.IsDefaultPort ? 5432 : uri.Port,
        Database = databaseName,
        Username = Uri.UnescapeDataString(userInfo[0]),
        Password = Uri.UnescapeDataString(userInfo[1]),
        SslMode = SslMode.Require,
        ChannelBinding = Npgsql.ChannelBinding.Require,
        Pooling = true,
        MaxPoolSize = 5,
        Timeout = 20,
        CommandTimeout = 30,
        ApplicationName = "Clima REST - Render"
    };

    return connection.ConnectionString;
}
