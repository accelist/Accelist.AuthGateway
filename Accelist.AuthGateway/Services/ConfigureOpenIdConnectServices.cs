using Accelist.AuthGateway.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Accelist.AuthGateway.Services
{
    public static class OpenIdConnectServiceConfigurationExtensions
    {
        public static WebApplicationBuilder ConfigureOpenIdConnectServices(this WebApplicationBuilder builder)
        {
            var configuration = builder.Configuration;
            var services = builder.Services;

            services.Configure<AuthGatewaySettings>(configuration.GetSection("AuthGateway"));
            services.AddTransient(di => di.GetRequiredService<IOptions<AuthGatewaySettings>>().Value);

            services.AddDbContextPool<AuthDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("DB"));
                options.UseOpenIddict();
            });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
               .AddCookie(options =>
               {
                   options.AccessDeniedPath = "/connect/signin";
                   options.LoginPath = "/connect/signin";
                   options.LogoutPath = "/connect/signout";
               });

            services.AddOpenIddict()

                // Register the OpenIddict core components.
                .AddCore(options =>
                {
                    // Configure OpenIddict to use the Entity Framework Core stores and models.
                    // Note: call ReplaceDefaultEntities() to replace the default entities.
                    options.UseEntityFrameworkCore()
                           .UseDbContext<AuthDbContext>();
                })

                // Register the OpenIddict server components.
                .AddServer(options =>
                {
                    // Enable the authorization, token, introspection and userinfo endpoints.
                    options.SetAuthorizationEndpointUris(configuration["OpenID:Endpoints:Authorization"])
                           .SetTokenEndpointUris(configuration["OpenID:Endpoints:Token"])
                           .SetIntrospectionEndpointUris(configuration["OpenID:Endpoints:Introspection"])
                           .SetUserinfoEndpointUris(configuration["OpenID:Endpoints:Userinfo"]);

                    // Enable the client credentials flow for machine to machine auth.
                    options.AllowClientCredentialsFlow();
                    // Enable the authorization code flow and refresh token flow for native and web apps.
                    options.AllowAuthorizationCodeFlow();
                    options.AllowRefreshTokenFlow();

                    // Expose all the supported claims in the discovery document.
                    options.RegisterClaims(configuration.GetSection("OpenID:Claims").Get<string[]>());

                    // Expose all the supported scopes in the discovery document.
                    options.RegisterScopes(configuration.GetSection("OpenID:Scopes").Get<string[]>());

                    // Register the signing and encryption credentials.
                    options.AddEphemeralEncryptionKey()
                           .AddEphemeralSigningKey();

                    // Register the ASP.NET Core host and configure the ASP.NET Core options.
                    options.UseAspNetCore()
                           // Note: the pass-through mode is not enabled for the token endpoint
                           // so that token requests are automatically handled by OpenIddict.
                           //.EnableTokenEndpointPassthrough();
                           .DisableTransportSecurityRequirement()
                           .EnableAuthorizationEndpointPassthrough();
                           //.EnableAuthorizationRequestCaching();
                })

                // Register the OpenIddict validation components.
                .AddValidation(options =>
                {
                    // Import the configuration from the local OpenIddict server instance.
                    options.UseLocalServer();

                    // Register the ASP.NET Core host.
                    options.UseAspNetCore();

                    // Enable authorization entry validation, which is required to be able
                    // to reject access tokens retrieved from a revoked authorization code.
                    options.EnableAuthorizationEntryValidation();
                });

            return builder;
        }
    }
}
