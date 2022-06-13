using Accelist.AuthGateway.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using System.Globalization;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Server.OpenIddictServerEvents;

namespace Accelist.AuthGateway.Services
{
    public static class OpenIdConnectServiceConfigurationExtensions
    {
        public static WebApplicationBuilder ConfigureOpenIdConnectServices(this WebApplicationBuilder builder)
        {
            var configuration = builder.Configuration;
            var services = builder.Services;
            var authGatewaySettings = new AuthGatewaySettings();
            configuration.GetSection("AuthGateway").Bind(authGatewaySettings);

            services.Configure<AuthGatewaySettings>(configuration.GetSection("AuthGateway"));
            services.AddTransient(di => di.GetRequiredService<IOptions<AuthGatewaySettings>>().Value);

            services.AddDbContextPool<AuthDbContext>(options =>
            {
                options.UseOpenIddict();
                options.UseSqlite("Data Source=AuthGateway.sqlite.db");
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("IdentityManagement", policy =>
                {
                    policy.RequireClaim(Claims.Scope, "identity-management");
                });
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
                    options.SetAuthorizationEndpointUris(OpenIdSettings.Endpoints.Authorization)
                           .SetTokenEndpointUris(OpenIdSettings.Endpoints.Token)
                           .SetIntrospectionEndpointUris(OpenIdSettings.Endpoints.Introspection)
                           .SetUserinfoEndpointUris(OpenIdSettings.Endpoints.Userinfo);

                    // Enable the client credentials flow for machine to machine auth.
                    options.AllowClientCredentialsFlow();
                    // Enable the authorization code flow and refresh token flow for native and web apps.
                    options.AllowAuthorizationCodeFlow();
                    options.AllowRefreshTokenFlow();

                    // Expose all the supported claims in the discovery document.
                    options.RegisterClaims(OpenIdSettings.Claims);

                    // Expose all the supported scopes in the discovery document.
                    options.RegisterScopes(OpenIdSettings.Scopes);

                    // Register the signing and encryption credentials.
                    options.AddEphemeralEncryptionKey()
                           .AddEphemeralSigningKey();

                    // Register the ASP.NET Core host and configure the ASP.NET Core options.
                    options.UseAspNetCore()
                           .DisableTransportSecurityRequirement()
                           .EnableAuthorizationEndpointPassthrough()
                           .EnableTokenEndpointPassthrough();

                    // Register the event handler responsible for populating userinfo responses.
                    options.AddEventHandler<HandleUserinfoRequestContext>(options =>
                        options.UseSingletonHandler<OpenIddictServerHandlers.PopulateUserinfo>()
                    );
                })

                // Register the OpenIddict validation components.
                .AddValidation(options =>
                {
                    // Import the configuration from the local OpenIddict server instance.
                    options.UseLocalServer();

                    // Register the ASP.NET Core host.
                    options.UseAspNetCore();

                    // Register the System.Net.Http integration.
                    //options.UseSystemNetHttp();

                    // Enable authorization entry validation, which is required to be able
                    // to reject access tokens retrieved from a revoked authorization code.
                    options.EnableAuthorizationEntryValidation();
                });

            return builder;
        }
    }
}
