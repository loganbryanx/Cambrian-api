using System.Text.Json;
using Npgsql;
using Stripe;
using Stripe.Checkout;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        var configured = builder.Configuration["CORS_ORIGINS"];
        var origins = string.IsNullOrWhiteSpace(configured)
            ? new[] { "http://localhost:5173", "http://localhost:5174" }
            : configured.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        policy.WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    }

app.UseHttpsRedirection();
app.UseCors("DevCors");

var users = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
var accounts = new Dictionary<string, AccountRecord>(StringComparer.OrdinalIgnoreCase);
var systemInfo = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
var secrets = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
var catalogsByEmail = new Dictionary<string, List<CatalogTrack>>(StringComparer.OrdinalIgnoreCase);
var uploadedTracksByEmail = new Dictionary<string, List<UploadedTrack>>(StringComparer.OrdinalIgnoreCase);
var playbackRequestsByEmail = new Dictionary<string, List<PlaybackRequestRecord>>(StringComparer.OrdinalIgnoreCase);
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
var playEventsConnectionString = builder.Configuration["PLAY_EVENTS_CONNECTION_STRING"]
    ?? builder.Configuration["CONNECTION_STRING"];
var playEventsStore = new PlayEventsStore(playEventsConnectionString);
var billingConnectionString = builder.Configuration["BILLING_CONNECTION_STRING"]
    ?? builder.Configuration["CONNECTION_STRING"];
var billingStore = new BillingStore(billingConnectionString);
await playEventsStore.EnsureTableAsync();
await billingStore.EnsureTablesAsync();

StripeConfiguration.ApiKey = builder.Configuration["STRIPE_SECRET_KEY"];

static bool IsStripeConfigured(IConfiguration config)
{
    return !string.IsNullOrWhiteSpace(config["STRIPE_SECRET_KEY"])
           && !string.IsNullOrWhiteSpace(config["STRIPE_LISTENER_PRICE_ID"])
           && !string.IsNullOrWhiteSpace(config["STRIPE_CREATOR_PRICE_ID"])
           && !string.IsNullOrWhiteSpace(config["STRIPE_SUCCESS_URL"])
           && !string.IsNullOrWhiteSpace(config["STRIPE_CANCEL_URL"])
           && !string.IsNullOrWhiteSpace(config["STRIPE_WEBHOOK_SECRET"]);
}

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

app.MapPost("/tracks/upload", (HttpRequest request, TrackUploadRequest payload) =>
{
    if (string.IsNullOrWhiteSpace(payload.Title) || string.IsNullOrWhiteSpace(payload.Artist))
    {
        return Results.BadRequest(new { message = "Title and artist are required." });
    }

    var email = ApiHelpers.GetEmail(request) ?? "member@cambrian.local";
    if (!uploadedTracksByEmail.TryGetValue(email, out var uploads))
    {
        uploads = new List<UploadedTrack>();
        uploadedTracksByEmail[email] = uploads;
    }

    var uploaded = new UploadedTrack(
        Guid.NewGuid().ToString("N"),
        email,
        payload.Title,
        payload.Artist,
        payload.Genre,
        payload.DurationSeconds,
        DateTimeOffset.UtcNow);

    uploads.Add(uploaded);

    if (!catalogsByEmail.TryGetValue(email, out var catalog))
    {
        catalog = new List<CatalogTrack>();
        catalogsByEmail[email] = catalog;
    }

    catalog.Add(new CatalogTrack(
        uploaded.Id,
        uploaded.Title,
        uploaded.Artist,
        uploaded.Genre ?? "Unspecified",
        payload.Price ?? 0m,
        "Creator-owned"));

    return Results.Ok(uploaded);
}).WithName("TrackUpload");

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

app.MapPost("/purchase/library", (HttpRequest request, SaveLibraryRequest payload) =>
{
    var email = ApiHelpers.GetEmail(request) ?? "member@cambrian.local";
    if (!licensesByEmail.TryGetValue(email, out var items))
    {
        items = new List<LicenseRecord>();
        licensesByEmail[email] = items;
    }

    var title = payload.Title ?? publishedCatalog.FirstOrDefault(t => t.Id == payload.TrackId)?.Title ?? "Saved track";
    items.Add(new LicenseRecord(Guid.NewGuid().ToString("N"), title, "Saved", DateOnly.FromDateTime(DateTime.UtcNow)));
    return Results.Ok(items);
}).WithName("PurchaseLibrarySave");

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

app.MapPost("/playback/request", async (HttpRequest request, PlaybackRequest payload) =>
{
    if (string.IsNullOrWhiteSpace(payload.TrackId))
    {
        return Results.BadRequest(new { message = "TrackId is required." });
    }

    var email = ApiHelpers.GetEmail(request) ?? "member@cambrian.local";
    if (!playbackRequestsByEmail.TryGetValue(email, out var requests))
    {
        requests = new List<PlaybackRequestRecord>();
        playbackRequestsByEmail[email] = requests;
    }

    var track = ApiHelpers.ResolveTrack(payload.TrackId, payload.Title, payload.Artist, catalogsByEmail, publishedCatalog);
    var record = new PlaybackRequestRecord(
        Guid.NewGuid().ToString("N"),
        email,
        payload.TrackId,
        track?.Title ?? payload.Title,
        track?.Artist ?? payload.Artist,
        payload.Device,
        payload.Source,
        DateTimeOffset.UtcNow);

    requests.Add(record);

    if (playEventsStore.IsConfigured)
    {
        await playEventsStore.WriteAsync(new PlayEventRecord(
            Guid.NewGuid().ToString("N"),
            DateTimeOffset.UtcNow,
            email,
            payload.TrackId,
            record.Title,
            record.Artist,
            payload.DurationSeconds,
            payload.Source ?? "playback-request"));
    }

    return Results.Ok(new { requestId = record.Id, status = "accepted" });
}).WithName("PlaybackRequest");

app.MapPost("/play/events", async (HttpRequest request, PlayEventRequest payload) =>
{
    if (string.IsNullOrWhiteSpace(payload.TrackId))
    {
        return Results.BadRequest(new { message = "TrackId is required." });
    }

    if (!playEventsStore.IsConfigured)
    {
        return Results.Problem("Play event storage is not configured.");
    }

    var email = ApiHelpers.GetEmail(request);
    var record = new PlayEventRecord(
        Guid.NewGuid().ToString("N"),
        DateTimeOffset.UtcNow,
        email,
        payload.TrackId,
        payload.Title,
        payload.Artist,
        payload.DurationSeconds,
        payload.Source);

    await playEventsStore.WriteAsync(record);
    return Results.Accepted();
}).WithName("PlayEvents");

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

app.MapPost("/billing/checkout-session", async (HttpRequest request, CheckoutRequest payload, IConfiguration config) =>
{
    if (!IsStripeConfigured(config))
    {
        return Results.Problem("Stripe is not configured.");
    }

    var email = ApiHelpers.GetEmail(request) ?? payload.Email ?? "member@cambrian.local";
    var planName = string.IsNullOrWhiteSpace(payload.Plan) ? "Listener" : payload.Plan;
    var priceId = planName.Equals("Creator", StringComparison.OrdinalIgnoreCase)
        ? config["STRIPE_CREATOR_PRICE_ID"]
        : config["STRIPE_LISTENER_PRICE_ID"];

    var options = new SessionCreateOptions
    {
        Mode = "subscription",
        SuccessUrl = config["STRIPE_SUCCESS_URL"],
        CancelUrl = config["STRIPE_CANCEL_URL"],
        CustomerEmail = email,
        LineItems = new List<SessionLineItemOptions>
        {
            new()
            {
                Price = priceId,
                Quantity = 1
            }
        },
        Metadata = new Dictionary<string, string>
        {
            ["plan"] = planName,
            ["email"] = email
        }
    };

    var service = new SessionService();
    var session = await service.CreateAsync(options);
    return Results.Ok(new { url = session.Url });
}).WithName("CreateCheckoutSession");

app.MapPost("/billing/webhook", async (HttpRequest request, IConfiguration config) =>
{
    if (!IsStripeConfigured(config))
    {
        return Results.Problem("Stripe is not configured.");
    }

    var json = await new StreamReader(request.Body).ReadToEndAsync();
    var signature = request.Headers["Stripe-Signature"].ToString();
    Event stripeEvent;

    try
    {
        stripeEvent = EventUtility.ConstructEvent(json, signature, config["STRIPE_WEBHOOK_SECRET"]);
    }
    catch (Exception)
    {
        return Results.BadRequest();
    }

    if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
    {
        if (stripeEvent.Data.Object is Session session)
        {
            var email = session.CustomerEmail ?? session.Metadata?.GetValueOrDefault("email");
            var planName = session.Metadata?.GetValueOrDefault("plan") ?? "Listener";
            if (!string.IsNullOrWhiteSpace(email))
            {
                var planDefinition = ApiHelpers.GetPlans().FirstOrDefault(p => p.Plan.Equals(planName, StringComparison.OrdinalIgnoreCase));
                var subscription = planDefinition != null
                    ? new SubscriptionRecord(planDefinition.Plan, "Active", DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)), planDefinition.PriceMonthly, planDefinition.Features)
                    : ApiHelpers.GetDefaultPlan();
                subscriptionsByEmail[email] = subscription;
                if (billingStore.IsConfigured)
                {
                    await billingStore.WriteSubscriptionAsync(email, subscription, session.Id, session.SubscriptionId);
                }
            }
        }
    }

    return Results.Ok();
}).WithName("StripeWebhook");

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
    if (billingStore.IsConfigured)
    {
        _ = billingStore.WriteSubscriptionAsync(email, updated, null, null);
    }
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

record SaveLibraryRequest(string TrackId, string? Title, string? Artist);

record TrackUploadRequest(string Title, string Artist, string? Genre, int? DurationSeconds, decimal? Price);

record PlaybackRequest(
    string TrackId,
    string? Title,
    string? Artist,
    int? DurationSeconds,
    string? Device,
    string? Source);

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

record CheckoutRequest(string? Plan, string? Email);

record PlayEventRequest(string TrackId, string? Title, string? Artist, int? DurationSeconds, string? Source);

record PlayEventRecord(
    string Id,
    DateTimeOffset OccurredAt,
    string? Email,
    string TrackId,
    string? Title,
    string? Artist,
    int? DurationSeconds,
    string? Source);

record UploadedTrack(
    string Id,
    string OwnerEmail,
    string Title,
    string Artist,
    string? Genre,
    int? DurationSeconds,
    DateTimeOffset UploadedAt);

record PlaybackRequestRecord(
    string Id,
    string Email,
    string TrackId,
    string? Title,
    string? Artist,
    string? Device,
    string? Source,
    DateTimeOffset RequestedAt);

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

    public static CatalogTrack? ResolveTrack(
        string trackId,
        string? title,
        string? artist,
        Dictionary<string, List<CatalogTrack>> catalogs,
        List<CatalogTrack> publishedCatalog)
    {
        foreach (var catalog in catalogs.Values)
        {
            var match = catalog.FirstOrDefault(t => t.Id == trackId);
            if (match != null)
            {
                return match;
            }
        }

        var published = publishedCatalog.FirstOrDefault(t => t.Id == trackId);
        if (published != null)
        {
            return published;
        }

        if (!string.IsNullOrWhiteSpace(title) || !string.IsNullOrWhiteSpace(artist))
        {
            return new CatalogTrack(trackId, title ?? "Unknown", artist ?? "Unknown", "Unknown", 0m, "Unknown");
        }

        return null;
    }
}

record PlanDefinition(string Plan, decimal PriceMonthly, string[] Features);

class BillingStore
{
    private readonly string? _connectionString;

    public BillingStore(string? connectionString)
    {
        _connectionString = connectionString;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_connectionString);

    public async Task EnsureTablesAsync()
    {
        if (!IsConfigured)
        {
            return;
        }

        await using var dataSource = NpgsqlDataSource.Create(_connectionString!);
        await using var cmd = dataSource.CreateCommand(@"
            CREATE TABLE IF NOT EXISTS subscriptions (
                id TEXT PRIMARY KEY,
                email TEXT NOT NULL,
                plan TEXT NOT NULL,
                status TEXT NOT NULL,
                renews_on DATE NOT NULL,
                price_monthly NUMERIC(10,2) NOT NULL,
                features JSONB NOT NULL,
                stripe_session_id TEXT NULL,
                stripe_subscription_id TEXT NULL,
                recorded_at TIMESTAMPTZ NOT NULL
            );
        ");
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task WriteSubscriptionAsync(string email, SubscriptionRecord subscription, string? sessionId, string? stripeSubscriptionId)
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException("Billing storage is not configured.");
        }

        var id = Guid.NewGuid().ToString("N");
        var featuresJson = JsonSerializer.Serialize(subscription.Features);

        await using var dataSource = NpgsqlDataSource.Create(_connectionString!);
        await using var cmd = dataSource.CreateCommand(@"
            INSERT INTO subscriptions (
                id,
                email,
                plan,
                status,
                renews_on,
                price_monthly,
                features,
                stripe_session_id,
                stripe_subscription_id,
                recorded_at
            ) VALUES (
                @id,
                @email,
                @plan,
                @status,
                @renews_on,
                @price_monthly,
                @features,
                @stripe_session_id,
                @stripe_subscription_id,
                @recorded_at
            );
        ");

        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("email", email);
        cmd.Parameters.AddWithValue("plan", subscription.Plan);
        cmd.Parameters.AddWithValue("status", subscription.Status);
        cmd.Parameters.AddWithValue("renews_on", subscription.RenewsOn.ToDateTime(TimeOnly.MinValue));
        cmd.Parameters.AddWithValue("price_monthly", subscription.PriceMonthly);
        cmd.Parameters.AddWithValue("features", NpgsqlTypes.NpgsqlDbType.Jsonb, featuresJson);
        cmd.Parameters.AddWithValue("stripe_session_id", (object?)sessionId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("stripe_subscription_id", (object?)stripeSubscriptionId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("recorded_at", DateTimeOffset.UtcNow);

        await cmd.ExecuteNonQueryAsync();
    }
}

class PlayEventsStore
{
    private readonly string? _connectionString;

    public PlayEventsStore(string? connectionString)
    {
        _connectionString = connectionString;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_connectionString);

    public async Task EnsureTableAsync()
    {
        if (!IsConfigured)
        {
            return;
        }

        await using var dataSource = NpgsqlDataSource.Create(_connectionString!);
        await using var cmd = dataSource.CreateCommand(@"
            CREATE TABLE IF NOT EXISTS play_events (
                id TEXT PRIMARY KEY,
                occurred_at TIMESTAMPTZ NOT NULL,
                email TEXT NULL,
                track_id TEXT NOT NULL,
                title TEXT NULL,
                artist TEXT NULL,
                duration_seconds INTEGER NULL,
                source TEXT NULL
            );
        ");
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task WriteAsync(PlayEventRecord record)
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException("Play events storage is not configured.");
        }

        await using var dataSource = NpgsqlDataSource.Create(_connectionString!);
        await using var cmd = dataSource.CreateCommand(@"
            INSERT INTO play_events (
                id,
                occurred_at,
                email,
                track_id,
                title,
                artist,
                duration_seconds,
                source
            ) VALUES (
                @id,
                @occurred_at,
                @email,
                @track_id,
                @title,
                @artist,
                @duration_seconds,
                @source
            );
        ");

        cmd.Parameters.AddWithValue("id", record.Id);
        cmd.Parameters.AddWithValue("occurred_at", record.OccurredAt);
        cmd.Parameters.AddWithValue("email", (object?)record.Email ?? DBNull.Value);
        cmd.Parameters.AddWithValue("track_id", record.TrackId);
        cmd.Parameters.AddWithValue("title", (object?)record.Title ?? DBNull.Value);
        cmd.Parameters.AddWithValue("artist", (object?)record.Artist ?? DBNull.Value);
        cmd.Parameters.AddWithValue("duration_seconds", (object?)record.DurationSeconds ?? DBNull.Value);
        cmd.Parameters.AddWithValue("source", (object?)record.Source ?? DBNull.Value);

        await cmd.ExecuteNonQueryAsync();
    }
}

