using Accelist.AuthGateway.Entities;
using Accelist.AuthGateway.Services;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.ConfigureOpenIdConnectServices();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
    db.Database.Migrate();

    if (await manager.FindByClientIdAsync("back-end") is null)
    {
        await manager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = "back-end",
            ClientSecret = "kMh86WtNTV5KZCdgpPYgAPFA8AudZwSm",
            DisplayName = "ASP.NET Core Web API",
            Permissions =
            {
                Permissions.Endpoints.Token,
                Permissions.Endpoints.Introspection,
                Permissions.Endpoints.Revocation,
                Permissions.GrantTypes.ClientCredentials,
                Permissions.Prefixes.Scope + "identity-management"
            },
            Type = ClientTypes.Confidential
        });
    }
    if (await manager.FindByClientIdAsync("front-end") is null)
    {
        await manager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = "front-end",
            DisplayName = "Next.js App",
            RedirectUris = {
                new Uri("http://localhost:3000/oauth2/callback")
            },
            Permissions =
            {
                Permissions.Endpoints.Token,
                Permissions.Endpoints.Authorization,
                Permissions.ResponseTypes.Code,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.GrantTypes.RefreshToken,
                Permissions.Scopes.Profile,
                Permissions.Scopes.Email,
                Permissions.Prefixes.Scope + "app"
            },
            Type = ClientTypes.Public
        });
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod()
);

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
