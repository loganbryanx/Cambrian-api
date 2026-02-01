var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("DevCors");

var users = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
var accounts = new Dictionary<string, AccountRecord>(StringComparer.OrdinalIgnoreCase);
var systemInfo = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
var secrets = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

app.MapGet("/auth/health", () => Results.Ok(new { status = "ok" }))
    .WithName("AuthHealth");

app.MapPost("/auth/register", (AuthRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest(new { message = "Email and password are required." });
    }

    if (users.ContainsKey(request.Email))
    {
        return Results.BadRequest(new { message = "Account already exists." });
    }

    users[request.Email] = request.Password;
    var account = new AccountRecord
    {
        Id = Guid.NewGuid().ToString("N"),
        Email = request.Email,
        Plan = "Creator",
        Region = "US",
        Status = "Active",
        Membership = "Verified"
    };
    accounts[request.Email] = account;

    return Results.Ok(new { token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()) });
}).WithName("Register");

app.MapPost("/auth/login", (AuthRequest request) =>
{
    if (!users.TryGetValue(request.Email ?? string.Empty, out var password) || password != request.Password)
    {
        return Results.Unauthorized();
    }

    return Results.Ok(new { token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()) });
}).WithName("Login");

app.MapGet("/data/account", (HttpRequest request) =>
{
    var email = request.Headers["x-email"].FirstOrDefault();
    if (!string.IsNullOrWhiteSpace(email) && accounts.TryGetValue(email, out var account))
    {
        return Results.Ok(account);
    }

    var fallback = accounts.Values.FirstOrDefault() ?? new AccountRecord
    {
        Id = Guid.NewGuid().ToString("N"),
        Email = "member@cambrian.local",
        Plan = "Creator",
        Region = "US",
        Status = "Active",
        Membership = "Verified"
    };

    return Results.Ok(fallback);
}).WithName("Account");

app.MapGet("/data/songs", () => Results.Ok(Array.Empty<object>())).WithName("Songs");
app.MapPost("/data/songs", (object payload) => Results.Ok(payload)).WithName("AddSong");

app.MapGet("/data/system", () => Results.Ok(systemInfo)).WithName("SystemInfo");
app.MapPost("/data/system", (SystemInfoRequest request) =>
{
    if (!string.IsNullOrWhiteSpace(request.Key))
    {
        systemInfo[request.Key] = request.Value ?? string.Empty;
    }
    return Results.Ok(systemInfo);
}).WithName("SetSystemInfo");

app.MapGet("/data/secrets", () => Results.Ok(secrets)).WithName("Secrets");
app.MapPost("/data/secrets", (SystemInfoRequest request) =>
{
    if (!string.IsNullOrWhiteSpace(request.Key))
    {
        secrets[request.Key] = request.Value ?? string.Empty;
    }
    return Results.Ok(secrets);
}).WithName("StoreSecret");

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

record AuthRequest(string Email, string Password);

record SystemInfoRequest(string Key, string? Value);

class AccountRecord
{
    public string? Id { get; set; }
    public string? Email { get; set; }
    public string? Plan { get; set; }
    public string? Region { get; set; }
    public string? Status { get; set; }
    public string? Membership { get; set; }
}
