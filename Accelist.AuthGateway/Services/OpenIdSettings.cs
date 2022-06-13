namespace Accelist.AuthGateway.Services
{
    public static class OpenIdSettings
    {
        public static class Endpoints
        {
            public static string Authorization { set; get; } = "/connect/authorize";

            public static string Introspection { set; get; } = "/connect/introspect";

            public static string Token { set; get; } = "/connect/token";

            public static string Userinfo { set; get; } = "/connect/userinfo";
        }

        public static string[] Claims { set; get; } = new string[]
        {
            OpenIddict.Abstractions.OpenIddictConstants.Claims.Address,
            OpenIddict.Abstractions.OpenIddictConstants.Claims.Birthdate,
            OpenIddict.Abstractions.OpenIddictConstants.Claims.Email,
            OpenIddict.Abstractions.OpenIddictConstants.Claims.EmailVerified,
            OpenIddict.Abstractions.OpenIddictConstants.Claims.FamilyName,
            OpenIddict.Abstractions.OpenIddictConstants.Claims.Gender,
            OpenIddict.Abstractions.OpenIddictConstants.Claims.GivenName,
            OpenIddict.Abstractions.OpenIddictConstants.Claims.Issuer,
            OpenIddict.Abstractions.OpenIddictConstants.Claims.Locale,
            OpenIddict.Abstractions.OpenIddictConstants.Claims.MiddleName,
            OpenIddict.Abstractions.OpenIddictConstants.Claims.Name,
            OpenIddict.Abstractions.OpenIddictConstants.Claims.Nickname,
            OpenIddict.Abstractions.OpenIddictConstants.Claims.PhoneNumber,
            OpenIddict.Abstractions.OpenIddictConstants.Claims.PhoneNumberVerified,
            OpenIddict.Abstractions.OpenIddictConstants.Claims.Picture,
            OpenIddict.Abstractions.OpenIddictConstants.Claims.PreferredUsername,
            OpenIddict.Abstractions.OpenIddictConstants.Claims.Profile,
            OpenIddict.Abstractions.OpenIddictConstants.Claims.Role,
            OpenIddict.Abstractions.OpenIddictConstants.Claims.Subject,
            OpenIddict.Abstractions.OpenIddictConstants.Claims.UpdatedAt,
            OpenIddict.Abstractions.OpenIddictConstants.Claims.Website,
            OpenIddict.Abstractions.OpenIddictConstants.Claims.Zoneinfo,
        };

        public static string[] Scopes { set; get; } = new string[]
        {
            OpenIddict.Abstractions.OpenIddictConstants.Scopes.Address,
            OpenIddict.Abstractions.OpenIddictConstants.Scopes.Email,
            OpenIddict.Abstractions.OpenIddictConstants.Scopes.OfflineAccess,
            OpenIddict.Abstractions.OpenIddictConstants.Scopes.OpenId,
            OpenIddict.Abstractions.OpenIddictConstants.Scopes.Phone,
            OpenIddict.Abstractions.OpenIddictConstants.Scopes.Profile,
            OpenIddict.Abstractions.OpenIddictConstants.Scopes.Roles,
        };
    }
}
