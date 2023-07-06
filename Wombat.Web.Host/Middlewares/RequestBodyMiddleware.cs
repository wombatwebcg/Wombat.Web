using Wombat;
using Wombat.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Web.Host
{

    public class RequestBodyMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestBodyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
             //context =  ACServiceProvider.GetService<HttpContext>();
            if ((context.Request.ContentType ?? string.Empty).Contains("application/json"))
            {
                context.Request.EnableBuffering();
                string body = await context.Request.Body?.ReadToStringAsync(Encoding.UTF8);
                context.RequestServices.GetService<RequestBody>().Body = body;
            }

            await _next(context);
        }
    }
}
