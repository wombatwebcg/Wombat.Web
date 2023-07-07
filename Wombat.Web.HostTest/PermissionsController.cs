
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Wombat.Web.Infrastructure;
using Wombat.Core.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Wombat.Web.Host.Filters;
using Wombat.Web.Host;

namespace Wombat.Web.HostTest
{
    [Route("/[controller]/[action]")]
    [OpenApiTag("获取权限")]
    public class PermissionsController : ApiControllerBase
    {
        private readonly JwtOptions _jwtOptions;
        private IServiceProvider _serviceProvider;
        public PermissionsController(IServiceProvider serviceProvider,
            IOptions<JwtOptions> jwtOptions
            )
        {
            _serviceProvider = serviceProvider;
            _jwtOptions = jwtOptions.Value;
        }

        [HttpGet]
        [AllowAnonymous]
        public string GetDevicesPermission()
        {
           var devcieKey= _serviceProvider.GetService<IConfiguration>().GetSection($"Permissions:Devices").Get<string>();
            var claims = new[]
            {
                new Claim("Devcies",devcieKey)
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var jwtToken = new JwtSecurityToken(
                string.Empty,
                string.Empty,
                claims,
                expires: DateTime.Now.AddHours(_jwtOptions.AccessExpireHours),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(jwtToken);

        }


        [HttpGet]
        public string TestPermission()
        {
            return "success";
        }
    }
}
