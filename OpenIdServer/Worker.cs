using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;
using System.Text.Json;
using OpenIdServer.Data;

namespace OpenIdServer
{
    public class Worker : IHostedService
    {

        private readonly IServiceProvider _serviceProvider;

        public Worker(IServiceProvider serviceProvider)
            => _serviceProvider = serviceProvider;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.EnsureCreatedAsync();

            var managerClient = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

            var managerScope = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

            if (await managerScope.FindByNameAsync("scope_1") is null)
                await managerScope.CreateAsync(new OpenIddictScopeDescriptor
                {

                    Name = "server_api_1",
                    DisplayName = "Scope 1",
                    Resources =
                        {
                            "server_api_1"
                        }
                });

            if (await managerClient.FindByClientIdAsync("client_console") is null)
            {
                var application = new OpenIddictApplicationDescriptor
                {
                    ClientId = "client_console",
                    ClientSecret = "388D45FA-B36B-4988-BA59-B187D329C207",
                    DisplayName = "My console client application",
                    Permissions =
                    {
                        Permissions.Endpoints.Token,
                        Permissions.GrantTypes.ClientCredentials,
                        Permissions.Prefixes.Scope + "server_api_1"
                    }
                };
                application.Properties.Add("prop1", JsonSerializer.SerializeToElement("prop1 value"));
                application.Properties.Add("prop2", JsonSerializer.SerializeToElement("prop2 value"));

                await managerClient.CreateAsync(application);
            }

            if (await managerClient.FindByClientIdAsync("server_api_1") is null)
            {
                await managerClient.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "server_api_1",
                    ClientSecret = "D86E100A-F1E1-42B9-9C6A-01AA669EF5DB",
                    DisplayName = "api server",                   
                    Permissions =
                    {
                        Permissions.Endpoints.Introspection,
                        Permissions.Endpoints.Authorization,
                    },
                    
                });
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
