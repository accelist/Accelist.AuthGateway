using Accelist.AuthGateway.Entities;
using Accelist.AuthGateway.Services;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Accelist.AuthGateway.Controllers
{
    [Route("connect/signin")]
    public class LoginConnectController : Controller
    {
        private readonly AuthDbContext DB;
        private readonly AuthGatewaySettings AuthGatewaySettings;

        public LoginConnectController(AuthDbContext authDbContext, AuthGatewaySettings authGatewaySettings)
        {
            this.DB = authDbContext;
            this.AuthGatewaySettings = authGatewaySettings;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string returnUrl)
        {
            var challenge = new LoginChallenge
            {
                LoginChallengeID = Ulid.NewUlid().ToString(),
                IsValid = true,
                ValidUntil = DateTimeOffset.UtcNow.AddMinutes(10),
                ReturnUrl = returnUrl
            };
            DB.LoginChallenge.Add(challenge);
            await DB.SaveChangesAsync();

            if (string.IsNullOrEmpty(AuthGatewaySettings.LoginPageUri))
            {
                throw new Exception("Auth Gateway setting: Login Page URL not found!");
            }

            var query = new QueryBuilder();
            query.Add("login_challenge", challenge.LoginChallengeID);

            var redirectTo = AuthGatewaySettings.LoginPageUri + query.ToQueryString();
            return Redirect(redirectTo);
        }
    }
}
