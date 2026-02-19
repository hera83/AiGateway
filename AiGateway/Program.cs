using AiGateway.Data;
using AiGateway.Endpoints;
using AiGateway.Middleware;
using AiGateway.Services;
using AiGateway.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerUI;

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
    var masterKey = config["Gateway:MasterKey"] ?? throw new InvalidOperationException("MasterKey not configured");
    var databasePath = config["Gateway:DatabasePath"] ?? throw new InvalidOperationException("DatabasePath not configured");
    var speachesBaseUrl = config["Upstreams:SpeachesBaseUrl"] ?? throw new InvalidOperationException("SpeachesBaseUrl not configured");
    var ollamaBaseUrl = config["Upstreams:OllamaBaseUrl"] ?? throw new InvalidOperationException("OllamaBaseUrl not configured");

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

    // Configure Ollama service
    builder.Services.Configure<OllamaOptions>(options =>
    {
        options.BaseUrl = ollamaBaseUrl;
        options.DefaultModel = config["Ollama:DefaultModel"] ?? "llama3.2";
        options.RequestTimeoutSeconds = int.TryParse(config["Ollama:RequestTimeoutSeconds"], out var timeout) ? timeout : 120;
        options.DefaultKeepAlive = config["Ollama:DefaultKeepAlive"];
    });
    
    builder.Services.AddHttpClient<IOllamaService, OllamaService>();

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
    });

    // Add authentication
    builder.Services.AddAuthentication("ApiKeyScheme")
        .AddScheme<ApiKeyAuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
            "ApiKeyScheme", 
            displayName: "API Key",
            configureOptions: _ => { });

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
    app.UseHttpsRedirection();
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

    // Health check endpoint
    app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
        .Produces(StatusCodes.Status200OK)
        .WithName("Health")
        .WithTags("System");

    Log.Information("AiGateway starting on {Urls}", string.Join(", ", app.Urls));
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
