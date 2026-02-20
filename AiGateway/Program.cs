using AiGateway.Data;
using AiGateway.Endpoints;
using AiGateway.Middleware;
using AiGateway.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Net;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddJsonFile("appsettings.Development.json", optional: true)
        .AddEnvironmentVariables()
        .Build())
    .Enrich.FromLogContext()
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseSerilog();

    // Read configuration
    var config = builder.Configuration;
    
    // For EF Core migrations to work, we need to handle missing config gracefully
    var masterKey = config["Gateway:MasterKey"];
    var databasePath = config["Gateway:DatabasePath"] ?? "data/apikeys.sqlite";
    var speachesBaseUrl = config["Upstreams:SpeachesBaseUrl"];
    var ollamaBaseUrl = config["Upstreams:OllamaBaseUrl"];
    var ollamaAuthorization = config["Upstreams:OllamaAuthorization"];
    var enableHttpsRedirection = config.GetValue<bool>("Gateway:EnableHttpsRedirection", true);
    var forceHttp11ForOllama = config.GetValue<bool>("Gateway:ForceHttp11ForOllama", false);
    var maxRequestBodyMb = config.GetValue<int>("Gateway:MaxRequestBodyMb", 300); // Default 300MB

    // Validate required config for actual runtime
    if (string.IsNullOrWhiteSpace(masterKey))
    {
        throw new InvalidOperationException("MasterKey not configured (set Gateway:MasterKey env var)");
    }
    if (string.IsNullOrWhiteSpace(speachesBaseUrl))
    {
        throw new InvalidOperationException("SpeachesBaseUrl not configured");
    }
    if (string.IsNullOrWhiteSpace(ollamaBaseUrl))
    {
        throw new InvalidOperationException("OllamaBaseUrl not configured");
    }

    Log.Information("Environment: {Environment}", builder.Environment.EnvironmentName);
    Log.Information("Upstreams: OllamaBaseUrl={OllamaBaseUrl}, SpeachesBaseUrl={SpeachesBaseUrl}", ollamaBaseUrl, speachesBaseUrl);
    Log.Information("Ollama ForceHttp11: {ForceHttp11}", forceHttp11ForOllama);
    Log.Information("Max request body size: {MaxMb}MB", maxRequestBodyMb);

    // Configure Kestrel max request body size
    builder.WebHost.ConfigureKestrel(options =>
    {
        // Set global max request body size (for most endpoints)
        options.Limits.MaxRequestBodySize = maxRequestBodyMb * 1024 * 1024; // Convert MB to bytes
    });

    // Ensure database directory exists
    var dbDir = Path.GetDirectoryName(databasePath);
    if (!string.IsNullOrEmpty(dbDir) && !Directory.Exists(dbDir))
    {
        Directory.CreateDirectory(dbDir);
    }

    // Add services
    builder.Services.AddDbContext<ApiKeyDbContext>(options =>
        options.UseSqlite($"Data Source={databasePath}"));
    builder.Services.AddScoped<IHashingService, HashingService>();
    builder.Services.AddScoped<IClientKeyService, ClientKeyService>();
    builder.Services.AddSingleton(masterKey);

    builder.Services.AddHttpClient("speaches", client =>
    {
        var baseUrl = speachesBaseUrl.EndsWith("/", StringComparison.Ordinal) ? speachesBaseUrl : $"{speachesBaseUrl}/";
        client.BaseAddress = new Uri(baseUrl);
        client.Timeout = Timeout.InfiniteTimeSpan;
    });

    builder.Services.AddHttpClient("ollama", client =>
    {
        var baseUrl = ollamaBaseUrl.EndsWith("/", StringComparison.Ordinal) ? ollamaBaseUrl : $"{ollamaBaseUrl}/";
        client.BaseAddress = new Uri(baseUrl);
        client.Timeout = Timeout.InfiniteTimeSpan;

        if (forceHttp11ForOllama)
        {
            client.DefaultRequestVersion = HttpVersion.Version11;
            client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;
        }
        
        // Add Authorization header to Ollama upstream if configured
        if (!string.IsNullOrWhiteSpace(ollamaAuthorization))
        {
            client.DefaultRequestHeaders.Add("Authorization", ollamaAuthorization);
            Log.Information("Ollama upstream Authorization header configured");
        }
    });

    // Add authentication
    builder.Services.AddAuthentication("ApiKeyScheme")
        .AddScheme<ApiKeyAuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
            "ApiKeyScheme", 
            displayName: "API Key",
            configureOptions: _ => { });

    // Add custom authorization failure handler
    // builder.Services.AddCustomAuthorizationHandler();  // Disabled for now - using middleware logging instead

    // Add authorization policies
    builder.Services.AddAuthorization(options =>
    {
        // Master only - must be master key and NOT client key
        options.AddPolicy("MasterOnly", policy =>
            policy.RequireAssertion(context =>
                context.User.FindFirst("is_master")?.Value == "true"));

        // Client only - must be client key and NOT master key
        options.AddPolicy("ClientOnly", policy =>
            policy.RequireAssertion(context =>
                context.User.FindFirst("is_master")?.Value == "false"));

        // Authenticated - either master or client
        options.AddPolicy("Authenticated", policy =>
            policy.RequireAssertion(context =>
                context.User.FindFirst("is_master") != null));
    });

    // Add Swagger
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "AiGateway API",
            Version = "v1",
            Description = "API Gateway with transparent request forwarding and API key management"
        });

        options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            Name = "x-api-key",
            In = ParameterLocation.Header,
            Description = "API key for authentication (GUID string)"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "ApiKey"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    var app = builder.Build();

    // Initialize database
    Log.Information("Initializing database at {DbPath}", databasePath);
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApiKeyDbContext>();
        dbContext.Database.Migrate();
        Log.Information("Database initialized successfully");
    }

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    // Use middleware
    app.UseErrorHandling();
    
    // Only enable HTTPS redirection when configured (disabled in container by default)
    if (enableHttpsRedirection)
    {
        app.UseHttpsRedirection();
    }
    
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseStatusCodeErrors();
    app.UseTrafficLogging();

    // Add Swagger UI
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "AiGateway API v1");
        options.DefaultModelsExpandDepth(-1); // Hide global schemas list
        options.DocExpansion(DocExpansion.None); // Collapse all endpoints by default
    });

    // Map endpoints
    app.MapKeyManagementEndpoints();
    app.MapSpeachesEndpoints();
    app.MapOllamaEndpoints();

    // Redirect root to Swagger UI for convenience
    app.MapGet("/", () => Results.Redirect("/swagger"))
        .ExcludeFromDescription();

    // Health check endpoint (public, no auth)
    app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
        .Produces(StatusCodes.Status200OK)
        .WithName("Health")
        .WithTags("System")
        .AllowAnonymous();

    // Log startup summary
    Log.Information("===== AiGateway Startup Summary =====");
    Log.Information("Environment: {Environment}", builder.Environment.EnvironmentName);
    Log.Information("Database: {DbPath}", databasePath);
    Log.Information("Upstreams: Ollama={Ollama}, Speaches={Speaches}", ollamaBaseUrl, speachesBaseUrl);
    Log.Information("Auth Policies: Master can access /v1/keys; Client can access /v1/ollama + /v1/speaches");
    Log.Information("Max request body: {MaxMb}MB", maxRequestBodyMb);
    Log.Information("Listening on: {Urls}", string.Join(", ", app.Urls));
    Log.Information("=====================================");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
