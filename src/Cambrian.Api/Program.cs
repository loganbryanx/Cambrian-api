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
var catalogsByEmail = new Dictionary<string, List<CatalogTrack>>(StringComparer.OrdinalIgnoreCase);
var publishedCatalog = new List<CatalogTrack>
{
    new CatalogTrack(Guid.NewGuid().ToString("N"), "Aurora Run", "Skyline Audio", "Ambient", 29m, "Creator-owned"),
    new CatalogTrack(Guid.NewGuid().ToString("N"), "Pulse Index", "Nova Loop", "Electro", 39m, "Creator-owned"),
    new CatalogTrack(Guid.NewGuid().ToString("N"), "Echo Bloom", "Signal North", "Chill", 25m, "Creator-owned")
};
var licensesByEmail = new Dictionary<string, List<LicenseRecord>>(StringComparer.OrdinalIgnoreCase);
var streamsByEmail = new Dictionary<string, List<StreamSession>>(StringComparer.OrdinalIgnoreCase);
var aiTracksByEmail = new Dictionary<string, List<AiTrack>>(StringComparer.OrdinalIgnoreCase);
var salesByEmail = new Dictionary<string, List<SaleRecord>>(StringComparer.OrdinalIgnoreCase);
var subscriptionsByEmail = new Dictionary<string, SubscriptionRecord>(StringComparer.OrdinalIgnoreCase);

app.MapGet("/auth/health", () => Results.Ok(new { status = "ok" }))
    .WithName("AuthHealth");

app.MapPost("/auth/register", (AuthRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest(new { message = "Email and password are required." });
    }

    var planName = string.IsNullOrWhiteSpace(request.Plan) ? "Listener" : request.Plan;
    var plan = ApiHelpers.GetPlans().FirstOrDefault(p => p.Plan.Equals(planName, StringComparison.OrdinalIgnoreCase));
    if (plan == null)
    {
        return Results.BadRequest(new { message = "Unknown plan." });
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
        Plan = plan.Plan,
        Region = "US",
        Status = "Active",
        Membership = plan.Plan
    };
    accounts[request.Email] = account;

    if (plan.Plan.Equals("Creator", StringComparison.OrdinalIgnoreCase))
    {
        ApiHelpers.SeedCreatorData(request.Email, catalogsByEmail, licensesByEmail, streamsByEmail, aiTracksByEmail, salesByEmail, subscriptionsByEmail);
    }
    else
    {
        ApiHelpers.SeedListenerData(request.Email, subscriptionsByEmail);
    }

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

app.MapGet("/catalog", (HttpRequest request) =>
{
    var email = ApiHelpers.GetEmail(request) ?? accounts.Keys.FirstOrDefault();
    if (email != null && catalogsByEmail.TryGetValue(email, out var items) && items.Count > 0)
    {
        return Results.Ok(items);
    }

    return Results.Ok(publishedCatalog);
}).WithName("Catalog");

app.MapGet("/discover", (HttpRequest request) =>
{
    var email = ApiHelpers.GetEmail(request) ?? accounts.Keys.FirstOrDefault();
    if (email != null && catalogsByEmail.TryGetValue(email, out var items) && items.Count > 0)
    {
        return Results.Ok(items.Take(6));
    }

    return Results.Ok(publishedCatalog.Take(6));
}).WithName("Discover");

app.MapGet("/purchase/health", () => Results.Ok(new { status = "ok" }))
    .WithName("PurchaseHealth");

app.MapGet("/purchase/library", (HttpRequest request) =>
{
    var email = ApiHelpers.GetEmail(request) ?? accounts.Keys.FirstOrDefault();
    var library = email != null && licensesByEmail.TryGetValue(email, out var items) ? items : new List<LicenseRecord>();
    return Results.Ok(library);
}).WithName("PurchaseLibrary");

app.MapGet("/stream", (HttpRequest request) =>
{
    var email = ApiHelpers.GetEmail(request) ?? accounts.Keys.FirstOrDefault();
    var sessions = email != null && streamsByEmail.TryGetValue(email, out var items) ? items : new List<StreamSession>();
    return Results.Ok(sessions);
}).WithName("StreamList");

app.MapPost("/stream/start", (HttpRequest request, StreamStartRequest payload) =>
{
    var email = ApiHelpers.GetEmail(request) ?? "member@cambrian.local";
    if (!streamsByEmail.TryGetValue(email, out var sessions))
    {
        sessions = new List<StreamSession>();
        streamsByEmail[email] = sessions;
    }

    var session = new StreamSession
    {
        Id = Guid.NewGuid().ToString("N"),
        Title = payload.Title ?? "Creator session",
        Status = "Live",
        StartedAt = DateTimeOffset.UtcNow
    };
    sessions.Add(session);
    return Results.Ok(session);
}).WithName("StreamStart");

app.MapPost("/stream/stop", (HttpRequest request, StreamStopRequest payload) =>
{
    var email = ApiHelpers.GetEmail(request);
    if (email != null && streamsByEmail.TryGetValue(email, out var sessions))
    {
        var session = sessions.FirstOrDefault(s => s.Id == payload.StreamId);
        if (session != null) session.Status = "Ended";
    }
    return Results.Ok(new { status = "stopped" });
}).WithName("StreamStop");

app.MapGet("/stream/community", (HttpRequest request) =>
{
    var email = ApiHelpers.GetEmail(request) ?? accounts.Keys.FirstOrDefault();
    var sessions = email != null && streamsByEmail.TryGetValue(email, out var items) ? items : new List<StreamSession>();
    return Results.Ok(sessions);
}).WithName("StreamCommunity");

app.MapGet("/ai/trending", (HttpRequest request) =>
{
    var email = ApiHelpers.GetEmail(request) ?? accounts.Keys.FirstOrDefault();
    var tracks = email != null && aiTracksByEmail.TryGetValue(email, out var items) ? items : new List<AiTrack>();
    return Results.Ok(tracks);
}).WithName("AiTrending");

app.MapGet("/ai/creator/tracks", (HttpRequest request) =>
{
    var email = ApiHelpers.GetEmail(request) ?? accounts.Keys.FirstOrDefault();
    var tracks = email != null && aiTracksByEmail.TryGetValue(email, out var items) ? items : new List<AiTrack>();
    return Results.Ok(tracks);
}).WithName("AiCreatorTracks");

app.MapGet("/ai/creator/revenue", (HttpRequest request) =>
{
    var email = ApiHelpers.GetEmail(request) ?? accounts.Keys.FirstOrDefault();
    var sales = email != null && salesByEmail.TryGetValue(email, out var items) ? items : new List<SaleRecord>();
    var amount = sales.Sum(s => s.Amount);
    return Results.Ok(new { amount });
}).WithName("AiCreatorRevenue");

app.MapGet("/marketplace/sales", (HttpRequest request) =>
{
    var email = ApiHelpers.GetEmail(request) ?? accounts.Keys.FirstOrDefault();
    var sales = email != null && salesByEmail.TryGetValue(email, out var items) ? items : new List<SaleRecord>();
    return Results.Ok(sales);
}).WithName("MarketplaceSales");

app.MapGet("/subscriptions/current", (HttpRequest request) =>
{
    var email = ApiHelpers.GetEmail(request);
    if (email != null && subscriptionsByEmail.TryGetValue(email, out var sub))
    {
        return Results.Ok(sub);
    }

    return Results.Ok(ApiHelpers.GetDefaultPlan());
}).WithName("SubscriptionCurrent");

app.MapGet("/subscriptions/plans", () => Results.Ok(ApiHelpers.GetPlans()))
    .WithName("SubscriptionPlans");

app.MapPost("/subscriptions/update", (HttpRequest request, SubscriptionUpdateRequest payload) =>
{
    var email = ApiHelpers.GetEmail(request) ?? "member@cambrian.local";
    var plan = ApiHelpers.GetPlans().FirstOrDefault(p => p.Plan.Equals(payload.Plan ?? string.Empty, StringComparison.OrdinalIgnoreCase));
    if (plan == null)
    {
        return Results.BadRequest(new { message = "Unknown plan." });
    }

    var updated = new SubscriptionRecord(plan.Plan, "Active", DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)), plan.PriceMonthly, plan.Features);
    subscriptionsByEmail[email] = updated;
    return Results.Ok(updated);
}).WithName("SubscriptionUpdate");

app.MapGet("/admin/audit", () => Results.Ok(new { status = "ok" }))
    .WithName("AdminAudit");

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

record AuthRequest(string Email, string Password, string? Plan);

record SystemInfoRequest(string Key, string? Value);

record StreamStartRequest(string TrackId, string? Title);

record StreamStopRequest(string StreamId);

record CatalogTrack(string Id, string Title, string Artist, string Genre, decimal Price, string Rights);

record LicenseRecord(string Id, string Title, string Status, DateOnly PurchasedOn);

class StreamSession
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? Status { get; set; }
    public DateTimeOffset StartedAt { get; set; }
}

record AiTrack(string Id, string Title, string Genre, string Tag);

record SaleRecord(string Id, string Title, decimal Amount, DateOnly SoldOn);

record SubscriptionRecord(string Plan, string Status, DateOnly RenewsOn, decimal PriceMonthly, string[] Features);

record SubscriptionUpdateRequest(string? Plan);

class AccountRecord
{
    public string? Id { get; set; }
    public string? Email { get; set; }
    public string? Plan { get; set; }
    public string? Region { get; set; }
    public string? Status { get; set; }
    public string? Membership { get; set; }
}

static class ApiHelpers
{
    public static string? GetEmail(HttpRequest request)
    {
        return request.Headers["x-email"].FirstOrDefault();
    }

    public static void SeedCreatorData(
        string email,
        Dictionary<string, List<CatalogTrack>> catalogs,
        Dictionary<string, List<LicenseRecord>> licenses,
        Dictionary<string, List<StreamSession>> streams,
        Dictionary<string, List<AiTrack>> aiTracks,
        Dictionary<string, List<SaleRecord>> sales,
        Dictionary<string, SubscriptionRecord> subscriptions)
    {
        if (!catalogs.ContainsKey(email))
        {
            catalogs[email] = new List<CatalogTrack>
            {
                new CatalogTrack(Guid.NewGuid().ToString("N"), "Neon Echoes", "Creator Studio", "Synthwave", 49m, "Creator-owned"),
                new CatalogTrack(Guid.NewGuid().ToString("N"), "Aurora Drift", "Creator Studio", "Ambient", 39m, "Creator-owned"),
                new CatalogTrack(Guid.NewGuid().ToString("N"), "Circuit Bloom", "Creator Studio", "Electro", 59m, "Creator-owned")
            };
        }

        if (!licenses.ContainsKey(email))
        {
            licenses[email] = new List<LicenseRecord>
            {
                new LicenseRecord(Guid.NewGuid().ToString("N"), "Neon Echoes", "Active", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2))),
                new LicenseRecord(Guid.NewGuid().ToString("N"), "Aurora Drift", "Active", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)))
            };
        }

        if (!streams.ContainsKey(email))
        {
            streams[email] = new List<StreamSession>
            {
                new StreamSession
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Title = "Creator Launch Stream",
                    Status = "Live",
                    StartedAt = DateTimeOffset.UtcNow.AddMinutes(-42)
                }
            };
        }

        if (!aiTracks.ContainsKey(email))
        {
            aiTracks[email] = new List<AiTrack>
            {
                new AiTrack(Guid.NewGuid().ToString("N"), "Midnight Atlas", "Synthwave", "Creator"),
                new AiTrack(Guid.NewGuid().ToString("N"), "Solar Bloom", "Ambient", "Creator")
            };
        }

        if (!sales.ContainsKey(email))
        {
            sales[email] = new List<SaleRecord>
            {
                new SaleRecord(Guid.NewGuid().ToString("N"), "Neon Echoes", 49m, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1))),
                new SaleRecord(Guid.NewGuid().ToString("N"), "Aurora Drift", 39m, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3)))
            };
        }

        if (!subscriptions.ContainsKey(email))
        {
            var creator = GetPlans().First(p => p.Plan == "Creator");
            subscriptions[email] = new SubscriptionRecord(creator.Plan, "Active", DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)), creator.PriceMonthly, creator.Features);
        }
    }

    public static SubscriptionRecord GetDefaultPlan()
    {
        var listener = GetPlans().First(p => p.Plan == "Listener");
        return new SubscriptionRecord(listener.Plan, "Active", DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)), listener.PriceMonthly, listener.Features);
    }

    public static void SeedListenerData(string email, Dictionary<string, SubscriptionRecord> subscriptions)
    {
        if (!subscriptions.ContainsKey(email))
        {
            subscriptions[email] = GetDefaultPlan();
        }
    }

    public static List<PlanDefinition> GetPlans()
    {
        return new List<PlanDefinition>
        {
            new PlanDefinition(
                "Listener",
                9m,
                new[]
                {
                    "Stream published creator catalogs",
                    "Curated listener playlists",
                    "Standard support"
                }
            ),
            new PlanDefinition(
                "Creator",
                29m,
                new[]
                {
                    "Publish creator-owned tracks",
                    "Sell licenses in marketplace",
                    "Live streaming monetization"
                }
            )
        };
    }
}

record PlanDefinition(string Plan, decimal PriceMonthly, string[] Features);
