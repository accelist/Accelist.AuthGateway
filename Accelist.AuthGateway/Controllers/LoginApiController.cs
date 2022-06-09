using Accelist.AuthGateway.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;
using System.ComponentModel.DataAnnotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Accelist.AuthGateway.Controllers
{
    public class LoginApiRequestModel
    {
        public string? Address { set; get; }

        public DateOnly? Birthdate { set; get; }

        public string? Email { set; get; }

        public bool? EmailVerified { set; get; }

        public string? FamilyName { set; get; }

        public string? Gender { set; get; }

        public string? GivenName { set; get; }

        public string? Locale { set; get; }

        public string? MiddleName { set; get; }

        public string? Name { set; get; }

        public string? Nickname { set; get; }

        public string? PhoneNumber { set; get; }

        public bool? PhoneNumberVerified { set; get; }

        public string? Picture { set; get; }

        public string? PreferredUsername { set; get; }

        public string? Profile { set; get; }

        [Required]
        public string Subject { set; get; } = "";

        public DateTime? UpdatedAt { set; get; }

        public string? Website { set; get; }

        public string? ZoneInfo { set; get; }

        public bool RememberMe { set; get; }

        [Required]
        public string LoginChallenge { set; get; } = "";
    }

    public class LoginApiResponseModel
    {
        public string RedirectTo { set; get; } = "";
    }

    [Route("api/login")]
    [ApiController]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme, Policy = "IdentityManagement")]
    public class LoginApiController : ControllerBase
    {
        private readonly AuthDbContext DB;

        public LoginApiController(AuthDbContext authDbContext)
        {
            this.DB = authDbContext;
        }

        [HttpPost]
        public async Task<ActionResult<LoginApiResponseModel>> Post([FromBody] LoginApiRequestModel model, CancellationToken cancellationToken)
        {
            var challenge = await DB.LoginChallenge.Where(Q =>
                Q.LoginChallengeID == model.LoginChallenge
                && Q.IsValid
                && Q.ValidUntil > DateTime.UtcNow
            ).FirstOrDefaultAsync(cancellationToken);

            if (challenge == null)
            {
                ModelState.AddModelError("model.LoginChallenge", "Invalid login challenge!");
                return ValidationProblem(ModelState);
            }

            if (ModelState.IsValid == false)
            {
                return ValidationProblem(ModelState);
            }

            var claims = new LoginClaims
            {
                LoginClaimsID = Ulid.NewUlid().ToString(),
                IsValid = true,
                ValidUntil = DateTime.UtcNow.AddMinutes(5),

                Address = model.Address,
                Birthdate = model.Birthdate,
                Email = model.Email,
                EmailVerified = model.EmailVerified,
                FamilyName = model.FamilyName,
                Gender = model.Gender,
                GivenName = model.GivenName,
                Locale = model.Locale,
                MiddleName = model.MiddleName,
                Name = model.Name,
                Nickname = model.Nickname,
                PhoneNumber = model.PhoneNumber,
                PhoneNumberVerified = model.PhoneNumberVerified,
                Picture = model.Picture,
                PreferredUsername = model.PreferredUsername,
                Profile = model.Profile,
                RememberMe = model.RememberMe,
                Subject = model.Subject,
                UpdatedAt = model.UpdatedAt,
                Website = model.Website,
                ZoneInfo = model.ZoneInfo,
                LoginChallengeID = challenge.LoginChallengeID
            };

            DB.LoginClaims.Add(claims);
            challenge.IsValid = false;
            await DB.SaveChangesAsync(cancellationToken);

            return new LoginApiResponseModel
            {
                RedirectTo = Request.PathBase + $"/authenticate?login_claims={claims.LoginClaimsID}"
            };
        }
    }
}
