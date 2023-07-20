using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;
using System.Security.Claims;
using System.Text.Json;

namespace OpenIdServer.Controllers
{
    public class AuthorizationController : Controller
    {
        private readonly IOpenIddictApplicationManager applicationManager;
        private readonly IOpenIddictScopeManager scopeManager;

        public AuthorizationController(IOpenIddictApplicationManager applicationManager, IOpenIddictScopeManager scopeManager)
        {
            this.applicationManager = applicationManager;
            this.scopeManager = scopeManager;
        }

        [HttpPost("~/connect/token"), Produces("application/json")]
        public async ValueTask<IActionResult> Exchange()
        {

            //retrieve OIDC request from original request
            var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            if (request.IsClientCredentialsGrantType())
            {
                var clientId = request.ClientId;
                var identity = new ClaimsIdentity(authenticationType: TokenValidationParameters.DefaultAuthenticationType);
                var application = await applicationManager.FindByClientIdAsync(clientId) ?? throw new InvalidOperationException("The application cannot be found.");

                // Use the client_id as the subject identifier.
                identity.SetClaim(Claims.Subject, await applicationManager.GetClientIdAsync(application));
                identity.SetClaim(Claims.Name, await applicationManager.GetDisplayNameAsync(application));

                identity.SetScopes(request.GetScopes());
                identity.SetResources(await scopeManager.ListResourcesAsync(identity.GetScopes()).ToListAsync());

                identity.SetAudiences("server_api_1");
                var properties = await applicationManager.GetPropertiesAsync(application);
                foreach (var item in properties.ToList())
                {
                    identity.SetClaim(item.Key, item.Value.ToString());
                }
                identity.SetClaim(Claims.Subject, clientId);
                identity.SetScopes(new[] { "scope_1" }.Intersect(request.GetScopes()));

                identity.SetDestinations(static claim => claim.Type switch
                {
                    // Allow the "name" claim to be stored in both the access and identity tokens
                    // when the "profile" scope was granted (by calling principal.SetScopes(...)).
                    Claims.Name when claim.Subject.HasScope(Scopes.Profile)
                        => new[] { Destinations.AccessToken, Destinations.IdentityToken },

                    // Otherwise, only store the claim in the access tokens.
                    _ => new[] { Destinations.AccessToken }
                });

                var principal = new ClaimsPrincipal(identity);
                // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
                return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            throw new NotImplementedException("The specified grant type is not implemented.");

        }
    }
}
