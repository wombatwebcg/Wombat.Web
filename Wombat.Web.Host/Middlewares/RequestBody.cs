
using Wombat.Core.DependencyInjection;
using Wombat.Web.Infrastructure;

namespace Wombat.Web.Host
{
    [Component(Lifetime = Core.DependencyInjection.ServiceLifetime.Singleton)]
    public class RequestBody
    {
        public string Body { get; set; }
    }
}
