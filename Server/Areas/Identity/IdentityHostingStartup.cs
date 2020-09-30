using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(Abeer.Server.Areas.Identity.IdentityHostingStartup))]
namespace Abeer.Server.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
            });
        }
    }
}