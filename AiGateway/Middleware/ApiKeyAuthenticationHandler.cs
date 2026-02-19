using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using AiGateway.Services;

namespace AiGateway.Middleware;

/// <summary>
/// Custom authentication scheme handler for x-api-key header.
/// </summary>
public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationSchemeOptions>
{
    private const string ApiKeyHeaderName = "x-api-key";
    private const string MasterKeyClaimName = "is_master";
    private const string KeyIdClaimName = "key_id";
    private const string AppNameClaimName = "app_name";

    private readonly string _masterKey;
    private readonly IClientKeyService _clientKeyService;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        string masterKey,
        IClientKeyService clientKeyService) : base(options, logger, encoder)
    {
        _masterKey = masterKey;
        _clientKeyService = clientKeyService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyValue))
        {
            return AuthenticateResult.NoResult();
        }

        var apiKey = apiKeyValue.ToString();

        // Check if it's the master key
        if (apiKey == _masterKey)
        {
            var masterTicket = CreateTicketForMasterKey();
            return AuthenticateResult.Success(masterTicket);
        }

        // Try to validate against client keys
        var clientKey = await _clientKeyService.ValidateAndGetKeyAsync(apiKey);
        if (clientKey != null && clientKey.Enabled)
        {
            // Update last used
            await _clientKeyService.UpdateLastUsedAsync(clientKey.Id);
            var clientTicket = CreateTicketForClientKey(clientKey.Id, clientKey.AppName);
            return AuthenticateResult.Success(clientTicket);
        }

        return AuthenticateResult.Fail("Invalid API key");
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        if (Response.HasStarted)
        {
            return Task.CompletedTask;
        }

        return ErrorResponseWriter.WriteAsync(
            Context,
            StatusCodes.Status401Unauthorized,
            "UNAUTHORIZED",
            "Missing or invalid API key");
    }

    protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        if (Response.HasStarted)
        {
            return Task.CompletedTask;
        }

        return ErrorResponseWriter.WriteAsync(
            Context,
            StatusCodes.Status403Forbidden,
            "FORBIDDEN",
            "Forbidden");
    }

    private AuthenticationTicket CreateTicketForMasterKey()
    {
        var claims = new List<Claim>
        {
            new Claim(MasterKeyClaimName, "true")
        };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        return new AuthenticationTicket(principal, Scheme.Name);
    }

    private AuthenticationTicket CreateTicketForClientKey(Guid keyId, string appName)
    {
        var claims = new List<Claim>
        {
            new Claim(MasterKeyClaimName, "false"),
            new Claim(KeyIdClaimName, keyId.ToString()),
            new Claim(AppNameClaimName, appName)
        };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        return new AuthenticationTicket(principal, Scheme.Name);
    }
}

public class ApiKeyAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
}
