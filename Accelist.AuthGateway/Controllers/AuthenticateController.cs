using Accelist.AuthGateway.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using static OpenIddict.Abstractions.OpenIddictConstants;
using System.Globalization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Accelist.AuthGateway.Controllers
{
    [Route("authenticate")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly AuthDbContext DB;

        public AuthenticateController(AuthDbContext authDbContext)
        {
            this.DB = authDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string login_claims,
            CancellationToken cancellationToken)
        {
            var userInfo = await DB.LoginClaims.Where(Q =>
                Q.LoginClaimsID == login_claims
                && Q.IsValid
                && DateTimeOffset.UtcNow < Q.ValidUntil)
                .Include(Q => Q.LoginChallenges)
                .FirstOrDefaultAsync(cancellationToken);

            if (userInfo == null)
            {
                ModelState.AddModelError("login_claims", "Invalid Login Claims ID.");
                return ValidationProblem(ModelState);
            }

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, Claims.Name, Claims.Role);
            var principal = new ClaimsPrincipal(identity);

            var time = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
            identity.AddClaim(new Claim(Claims.AuthenticationTime, time, ClaimValueTypes.Integer64));

            if (userInfo.Address != null)
            {
                identity.AddClaim(new Claim(Claims.Address, userInfo.Address));
            }
            var birthdateISO = userInfo.Birthdate?.ToString("o");
            if (string.IsNullOrEmpty(birthdateISO) == false)
            {
                identity.AddClaim(new Claim(Claims.Birthdate, birthdateISO));
            }
            if (userInfo.Email != null)
            {
                identity.AddClaim(new Claim(Claims.Email, userInfo.Email));
            }
            if (userInfo.EmailVerified != null)
            {
                identity.AddClaim(new Claim(Claims.EmailVerified, userInfo.EmailVerified.ToString() ?? "false", ClaimValueTypes.Boolean));
            }
            if (userInfo.FamilyName != null)
            {
                identity.AddClaim(new Claim(Claims.FamilyName, userInfo.FamilyName));
            }
            if (userInfo.Gender != null)
            {
                identity.AddClaim(new Claim(Claims.Gender, userInfo.Gender));
            }
            if (userInfo.GivenName != null)
            {
                identity.AddClaim(new Claim(Claims.GivenName, userInfo.GivenName));
            }
            if (userInfo.Locale != null)
            {
                identity.AddClaim(new Claim(Claims.Locale, userInfo.Locale));
            }
            if (userInfo.MiddleName != null)
            {
                identity.AddClaim(new Claim(Claims.MiddleName, userInfo.MiddleName));
            }
            if (userInfo.Name != null)
            {
                identity.AddClaim(new Claim(Claims.Name, userInfo.Name));
            }
            if (userInfo.Nickname != null)
            {
                identity.AddClaim(new Claim(Claims.Nickname, userInfo.Nickname));
            }
            if (userInfo.PhoneNumber != null)
            {
                identity.AddClaim(new Claim(Claims.PhoneNumber, userInfo.PhoneNumber));
            }
            if (userInfo.PhoneNumberVerified != null)
            {
                identity.AddClaim(new Claim(Claims.PhoneNumberVerified, userInfo.PhoneNumberVerified.ToString() ?? "false", ClaimValueTypes.Boolean));
            }
            if (userInfo.Picture != null)
            {
                identity.AddClaim(new Claim(Claims.Picture, userInfo.Picture));
            }
            if (userInfo.PreferredUsername != null)
            {
                identity.AddClaim(new Claim(Claims.PreferredUsername, userInfo.PreferredUsername));
            }
            if (userInfo.Profile != null)
            {
                identity.AddClaim(new Claim(Claims.Profile, userInfo.Profile));
            }
            identity.AddClaim(new Claim(Claims.Subject, userInfo.Subject));

            if (userInfo.UpdatedAt != null)
            {
                DateTimeOffset updatedAtDto = DateTime.SpecifyKind(userInfo.UpdatedAt.Value, DateTimeKind.Utc);
                var updatedAtUnix = updatedAtDto.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
                identity.AddClaim(new Claim(Claims.UpdatedAt, updatedAtUnix, ClaimValueTypes.Integer64));
            }
            if (userInfo.Website != null)
            {
                identity.AddClaim(new Claim(Claims.Profile, userInfo.Website));
            }
            if (userInfo.ZoneInfo != null)
            {
                identity.AddClaim(new Claim(Claims.Profile, userInfo.ZoneInfo));
            }

            userInfo.IsValid = false;
            await DB.SaveChangesAsync(cancellationToken);

            var challenge = userInfo.LoginChallenges.First();
            
            return SignIn(principal, new AuthenticationProperties
            {
                IsPersistent = userInfo.RememberMe,
                RedirectUri = challenge.ReturnUrl
            });
        }
    }
}
