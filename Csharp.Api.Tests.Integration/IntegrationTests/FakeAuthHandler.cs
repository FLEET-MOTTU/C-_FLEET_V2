using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Csharp.Api.Tests.Integration
{
    public class FakeAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public FakeAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger, UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // O sub (NameIdentifier) é o telefone como no Java
            var claims = new List<Claim>
            {
                // NameIdentifier remains an example value; Name should be the telefone used in seeded data
                new Claim(ClaimTypes.NameIdentifier, "123456789"),
                new Claim(ClaimTypes.Role, "OPERACIONAL"),
                new Claim(ClaimTypes.Name, "11987654331"), // matches seeded Funcionario.Telefone
            };
            
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "TestScheme");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}