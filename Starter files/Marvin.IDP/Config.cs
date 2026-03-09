using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace Marvin.IDP;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
         [
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResource("roles",
                "Your role(s)",
                ["role"]),
            new IdentityResource("country",
                "The country you're living in",
                ["country"])
         ];

    public static IEnumerable<ApiResource> ApiResources =>
     [
             new ApiResource("imagegalleryapi",
                 "Image Gallery API",
                 ["role", "country"])
             {
                 Scopes = { "imagegalleryapi.fullaccess",
                     "imagegalleryapi.read",
                     "imagegalleryapi.write"}               
             }
         ];


    public static IEnumerable<ApiScope> ApiScopes =>
        [
                new ApiScope("imagegalleryapi.fullaccess"),
                new ApiScope("imagegalleryapi.read"),
                new ApiScope("imagegalleryapi.write")];

    public static IEnumerable<Client> Clients =>
        [
                new Client()
                {
                    ClientName = "Image Gallery",
                    ClientId = "imagegalleryclient",
                    AllowedGrantTypes = GrantTypes.Code,
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "roles",
                        "imagegalleryapi.fullaccess",
                        "imagegalleryapi.read",
                        "imagegalleryapi.write",
                        "country"
                    }, 
                    RedirectUris =
                    {
                        "https://localhost:7184/signin-oidc"
                    },        
                    RequireConsent = false,
                    //PostLogoutRedirectUris =
                    //{
                    //    "https://localhost:7184/signout-callback-oidc"
                    //},
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowOfflineAccess = true,
                    UpdateAccessTokenClaimsOnRefresh = true            
                }
            ];
}
