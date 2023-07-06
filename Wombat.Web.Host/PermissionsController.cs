
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

namespace Wombat.Web.Host
{
    [Route("/[controller]/[action]")]
    [OpenApiTag("获取权限")]
    public class PermissionsController : ApiControllerBase
    {
        private readonly JwtOptions _jwtOptions;
        public PermissionsController(
            IOptions<JwtOptions> jwtOptions
            )
        {
            _jwtOptions = jwtOptions.Value;
        }

        [HttpGet]
        [AllowAnonymous]
        public string GetDevicesPermission()
        {
           var devcieKey= CustomServiceProvider.GetConfiguration().GetSection($"Permissions:Devices").Get<string>();
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
