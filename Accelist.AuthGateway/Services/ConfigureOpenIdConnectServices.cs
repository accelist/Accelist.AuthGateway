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
                           .DisableTransportSecurityRequirement()
                           .EnableAuthorizationEndpointPassthrough()
                           .EnableTokenEndpointPassthrough();

                    // Register the event handler responsible for populating userinfo responses.
                    options.AddEventHandler<HandleUserinfoRequestContext>(options =>
                        options.UseInlineHandler(context =>
                        {
                            if (context.Principal == null)
                            {
                                return default;
                            }

                            if (context.Principal.HasScope(Scopes.Profile))
                            {
                                context.GivenName = context.Principal.GetClaim(Claims.GivenName);
                                context.FamilyName = context.Principal.GetClaim(Claims.FamilyName);
                                context.BirthDate = context.Principal.GetClaim(Claims.Birthdate);
                                context.Profile = context.Principal.GetClaim(Claims.Profile);
                                context.PreferredUsername = context.Principal.GetClaim(Claims.PreferredUsername);
                                context.Website = context.Principal.GetClaim(Claims.Website);

                                context.Claims[Claims.Name] = context.Principal.GetClaim(Claims.Name);
                                context.Claims[Claims.Gender] = context.Principal.GetClaim(Claims.Gender);
                                context.Claims[Claims.MiddleName] = context.Principal.GetClaim(Claims.MiddleName);
                                context.Claims[Claims.Nickname] = context.Principal.GetClaim(Claims.Nickname);
                                context.Claims[Claims.Picture] = context.Principal.GetClaim(Claims.Picture);
                                context.Claims[Claims.Locale] = context.Principal.GetClaim(Claims.Locale);
                                context.Claims[Claims.Zoneinfo] = context.Principal.GetClaim(Claims.Zoneinfo);
                                context.Claims[Claims.UpdatedAt] = long.Parse(
                                    context.Principal.GetClaim(Claims.UpdatedAt) ?? "0",
                                    NumberStyles.Number, CultureInfo.InvariantCulture);
                            }

                            if (context.Principal.HasScope(Scopes.Email))
                            {
                                context.Email = context.Principal.GetClaim(Claims.Email);
                                context.EmailVerified = bool.Parse(context.Principal.GetClaim(Claims.EmailVerified) ?? "false");
                            }

                            if (context.Principal.HasScope(Scopes.Phone))
                            {
                                context.PhoneNumber = context.Principal.GetClaim(Claims.PhoneNumber);
                                context.PhoneNumberVerified = bool.Parse(context.Principal.GetClaim(Claims.PhoneNumberVerified) ?? "false");
                            }

                            return default;
                        })
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
