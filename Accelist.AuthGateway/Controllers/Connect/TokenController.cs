using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Authentication;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Accelist.AuthGateway.Controllers.Connect
{
    [Route("connect/token")]
    public class TokenController : Controller
    {
        private readonly IOpenIddictApplicationManager AppManager;
        private readonly IOpenIddictScopeManager ScopeManager;

        public TokenController(IOpenIddictApplicationManager applicationManager, IOpenIddictScopeManager scopeManager)
        {
            AppManager = applicationManager;
            ScopeManager = scopeManager;
        }

        [HttpPost]
        public async Task<IActionResult> Exchange()
        {
            var request = HttpContext.GetOpenIddictServerRequest();
            if (request?.ClientId == null)
            {
                throw new InvalidOperationException("Unable to retrieve the OpenIddict Server request instance.");
            }

            if (request.IsClientCredentialsGrantType())
            {
                // Note: the client credentials are automatically validated by OpenIddict:
                // if client_id or client_secret are invalid, this action won't be invoked.

                var application = await AppManager.FindByClientIdAsync(request.ClientId);
                if (application == null)
                {
                    throw new InvalidOperationException("The application details cannot be found in the database.");
                }

                var clientId = await AppManager.GetClientIdAsync(application);
                var displayName = await AppManager.GetDisplayNameAsync(application);
                if (clientId == null || displayName == null)
                {
                    throw new InvalidOperationException("The application Client ID and/or Display Name is not found in the database.");
                }

                // Create a new ClaimsIdentity containing the claims that
                // will be used to create an id_token, a token or a code.
                var identity = new ClaimsIdentity(
                    TokenValidationParameters.DefaultAuthenticationType,
                    Claims.Name, Claims.Role);

                // Use the client_id as the subject identifier.
                identity.AddClaim(Claims.Subject, clientId,
                    Destinations.AccessToken, Destinations.IdentityToken);

                identity.AddClaim(Claims.Name, displayName,
                    Destinations.AccessToken, Destinations.IdentityToken);

                // Note: In the original OAuth 2.0 specification, the client credentials grant
                // doesn't return an identity token, which is an OpenID Connect concept.
                //
                // As a non-standardized extension, OpenIddict allows returning an id_token
                // to convey information about the client application when the "openid" scope
                // is granted (i.e specified when calling principal.SetScopes()). When the "openid"
                // scope is not explicitly set, no identity token is returned to the client application.

                // Set the list of scopes granted to the client application in access_token.
                var principal = new ClaimsPrincipal(identity);
                principal.SetScopes(request.GetScopes());
                principal.SetResources(await ScopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync());

                foreach (var claim in principal.Claims)
                {
                    claim.SetDestinations(GetDestinationsForClientCredentials(claim));
                }

                return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
            {
                // Retrieve the claims principal stored in the authorization code/device code/refresh token.
                var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                var principal = result?.Principal;
                if (principal == null)
                {
                    throw new InvalidOperationException("Unable to retrieve Claims Principal from authorization code / device code / refresh token.");
                }

                // Retrieve the user profile corresponding to the authorization code/refresh token.
                // Note: if you want to automatically invalidate the authorization code/refresh token
                // when the user password/roles change, use the following line instead:
                // var user = _signInManager.ValidateSecurityStampAsync(info.Principal);
                //var user = await _userManager.GetUserAsync(principal);
                //if (user == null)
                //{
                //    return Forbid(
                //        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                //        properties: new AuthenticationProperties(new Dictionary<string, string>
                //        {
                //            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                //            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The token is no longer valid."
                //        }));
                //}

                //// Ensure the user is still allowed to sign in.
                //if (!await _signInManager.CanSignInAsync(user))
                //{
                //    return Forbid(
                //        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                //        properties: new AuthenticationProperties(new Dictionary<string, string>
                //        {
                //            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                //            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is no longer allowed to sign in."
                //        }));
                //}

                foreach (var claim in principal.Claims)
                {
                    claim.SetDestinations(GetDestinationsForAuthorizationCode(claim, principal));
                }

                // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
                return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            throw new NotImplementedException("The specified grant type is not implemented.");
        }

        private static IEnumerable<string> GetDestinationsForClientCredentials(Claim claim)
        {
            // Note: by default, claims are NOT automatically included in the access and identity tokens.
            // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
            // whether they should be included in access tokens, in identity tokens or in both.

            return claim.Type switch
            {
                Claims.Name or
                Claims.Subject
                    => ImmutableArray.Create(Destinations.AccessToken, Destinations.IdentityToken),

                _ => ImmutableArray.Create(Destinations.AccessToken),
            };
        }

        private static IEnumerable<string> GetDestinationsForAuthorizationCode(Claim claim, ClaimsPrincipal principal)
        {
            // Note: by default, claims are NOT automatically included in the access and identity tokens.
            // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
            // whether they should be included in access tokens, in identity tokens or in both.

            switch (claim.Type)
            {
                case Claims.Name:
                    yield return Destinations.AccessToken;

                    if (principal.HasScope(Scopes.Profile))
                        yield return Destinations.IdentityToken;

                    yield break;

                case Claims.Email:
                    yield return Destinations.AccessToken;

                    if (principal.HasScope(Scopes.Email))
                        yield return Destinations.IdentityToken;

                    yield break;

                case Claims.Role:
                    yield return Destinations.AccessToken;

                    if (principal.HasScope(Scopes.Roles))
                        yield return Destinations.IdentityToken;

                    yield break;

                // Never include the security stamp in the access and identity tokens, as it's a secret value.
                case "AspNet.Identity.SecurityStamp": yield break;

                default:
                    yield return Destinations.AccessToken;
                    yield break;
            }
        }
    }
}
