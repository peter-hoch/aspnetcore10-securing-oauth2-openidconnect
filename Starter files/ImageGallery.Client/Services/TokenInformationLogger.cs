using Microsoft.AspNetCore.Authentication;
using System.Text;

namespace ImageGallery.Client.Services;

/// <summary>
/// Helper class to log token and identity information.  DO NOT use this in
/// production, this is for demo purposes only!  You must NOT log sensitive
/// information like tokens in real-life applications, that's a security risk.
/// </summary>
/// <param name="logger">The logger</param>
/// <param name="httpContextAccessor">HttpContextAccessor, required for accessing HttpContext</param>
/// <param name="schemeProvider">AuthenticationSchemeProvider, required for validating if a scheme has been registered</param>
public class TokenInformationLogger(ILogger<TokenInformationLogger> logger,
    IHttpContextAccessor httpContextAccessor,
    IAuthenticationSchemeProvider schemeProvider) : ITokenInformationLogger
{
    public async Task Log()
    {
        var userClaims = string.Join('\n', httpContextAccessor.HttpContext?.User.Claims
            .Select(c => $"Claim type: {c.Type} - Claim value: {c.Value}") ?? []);

        logger.LogInformation($"User claims: \n{userClaims}");

        await LogTokenAsync("Identity token", "id_token");
        await LogTokenAsync("Access token", "access_token");
        await LogTokenAsync("Refresh token", "refresh_token");
    }

    private async Task LogTokenAsync(string tokenName, string tokenType)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return;
        }

        if (!(await schemeProvider.GetAllSchemesAsync()).Any())
        {
            // if there's no authentication scheme registered, 
            // there are no tokens so we also cannot get
            // them (GetTokenAsync will throw an Exception)
            return;
        }
 
        var token = await httpContext.GetTokenAsync(tokenType);
        if (token is not null)
        {
            logger.LogInformation($"{tokenName}: \n{token}");
        }
    }
}