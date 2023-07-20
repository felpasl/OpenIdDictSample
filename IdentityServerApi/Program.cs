using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System.IdentityModel.Tokens.Jwt;


Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Code)
            .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
IdentityModelEventSource.ShowPII = true;

builder.Services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddIdentityServerAuthentication("OAuth", options =>
            {
                options.Authority = "http://openidserver/";
                options.RequireHttpsMetadata = false;
                options.ApiName = "server_api_1";
                options.ApiSecret = "D86E100A-F1E1-42B9-9C6A-01AA669EF5DB";
                
                options.JwtBearerEvents = new JwtBearerEvents
                {
                    OnTokenValidated = e =>
                    {
                        var jwt = e.SecurityToken as JwtSecurityToken;
                        var type = jwt.Header.Typ;

                        if (!string.Equals(type, "at+jwt", StringComparison.Ordinal))
                        {
                            e.Fail("JWT is not an access token");
                        }

                        return Task.CompletedTask;
                    }, OnMessageReceived = e => { 

                        e.Options.TokenValidationParameters.ValidIssuers = new List<string> { "http://openidserver/", "http://localhost:3030/" };

                        return Task.CompletedTask;
                    }
                };
            });

builder.Services.AddAuthorization(auth =>
{
    auth.AddPolicy("OAuth", new AuthorizationPolicyBuilder()
       .AddAuthenticationSchemes("OAuth")
       .RequireAuthenticatedUser().Build());
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
