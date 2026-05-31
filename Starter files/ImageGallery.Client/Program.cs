using Duende.AccessTokenManagement.OpenIdConnect;
using ImageGallery.Authorization;
using ImageGallery.Client.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(configure => 
        configure.JsonSerializerOptions.PropertyNamingPolicy = null);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITokenInformationLogger, TokenInformationLogger>();
builder.Services.AddOpenIdConnectAccessTokenManagement();

// add support for policies
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("UserCanAddImage", AuthorizationPolicies.CanAddImage());

// create an HttpClient used for accessing the API
builder.Services.AddHttpClient("APIClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ImageGalleryAPIRoot"] ??
                                 throw new InvalidOperationException("Configuration setting ImageGalleryAPIRoot is missing."));
    client.DefaultRequestHeaders.Clear();
    client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
}).AddUserAccessTokenHandler();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    }).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.AccessDeniedPath = "/Authentication/AccessDenied";
    })
    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.Authority = "https://localhost:5001";
        options.ClientId = "imagegalleryclient";
        options.ResponseType = "code";
        //options.CallbackPath = new PathString("...");
        //options.SignedOutCallbackPath = new PathString("...");  // the callback for the IPD sign out - the default is host:port/signout-callback-oidc AND in or in config section Marvin.IDP\Config.cs
        options.Scope.Add(("openid"));  // default value
        options.Scope.Add("profile");  // default value
        options.Scope.Add("roles");
        // add scope
        options.Scope.Add("imagegalleryapi.write");
        // add scope for country
        options.Scope.Add("country");
        options.SaveTokens = true;
        options.ClientSecret = "secret";
        // get additional claims from the UerInfoEndpoint
        options.GetClaimsFromUserInfoEndpoint = true;
        // do not map claims to old standards
        options.MapInboundClaims = false;
        // add the to the claims
        options.ClaimActions.Remove("aud");
        // remove claims
        options.ClaimActions.DeleteClaims("sid", "idp");
        // and add a claim mapping
        options.ClaimActions.MapJsonKey("role", "role");
        // and add a claim mapping
        options.ClaimActions.MapUniqueJsonKey("country", "country");
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            NameClaimType = "given_name",
            RoleClaimType = "role"
        };
    });

// do not map claims to old standards - to handle multiple IDPs
//JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Gallery}/{action=Index}/{id?}");

app.Run();
