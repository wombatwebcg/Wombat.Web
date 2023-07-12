using Wombat.Web.Host;
namespace Wombat.Web.HostTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.ServicesBuild();

            var app = builder.Build();

            app.AppBuild();

            app.Run();
        }
    }
}