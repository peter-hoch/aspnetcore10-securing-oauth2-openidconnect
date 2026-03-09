using Duende.AccessTokenManagement;
using Duende.AccessTokenManagement.OpenIdConnect;
using Duende.Bff;
using Duende.Bff.AccessTokenManagement;
using Duende.Bff.Yarp;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(configure =>
        configure.JsonSerializerOptions.PropertyNamingPolicy = null);

builder.Services.AddOpenIdConnectAccessTokenManagement();

builder.Services.AddBff().AddRemoteApis();

const string bffCookieScheme = "BFFCookieScheme";
const string bffChallengeScheme = "BFFChallengeScheme";

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = bffCookieScheme;
    options.DefaultChallengeScheme = bffChallengeScheme;
}).AddCookie(bffCookieScheme, options =>
{
    options.Events.OnSigningOut = async e => { await e.HttpContext.RevokeRefreshTokenAsync(); };
})
.AddOpenIdConnect(bffChallengeScheme, options =>
{
    options.SignInScheme = bffCookieScheme;
    options.Authority = "https://localhost:5001/";
    options.ClientId = "imagegallerybff";
    options.ClientSecret = "anothersecret";
    options.ResponseType = "code";
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.Scope.Add("roles");
    options.Scope.Add("imagegalleryapi.read");
    options.Scope.Add("imagegalleryapi.write");
    options.Scope.Add("country");
    options.Scope.Add("offline_access");
    options.ClaimActions.MapJsonKey("role", "role");
    options.ClaimActions.MapUniqueJsonKey("country", "country");
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new()
    {
        NameClaimType = "given_name",
        RoleClaimType = "role",
    };
});




var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRemoteBffApiEndpoint("/bff/images",
    new Uri("https://localhost:7075/api/images"))
    .WithAccessToken(RequiredTokenType.User);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
