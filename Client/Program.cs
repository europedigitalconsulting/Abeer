using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using BlazorAnimate;
using Microsoft.JSInterop;
using Abeer.Client.UISdk;
using Tewr.Blazor.FileReader;

namespace Abeer.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.RootComponents.Add<App>("app");

            builder.Services.AddHttpClient("Abeer.Anonymous", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));

            builder.Services.AddHttpClient("Abeer.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

            // Supply HttpClient instances that include access tokens when making requests to the server project
            builder.Services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Abeer.ServerAPI"));
            
            builder.Services.AddLocalization();
            builder.Services.AddApiAuthorization();
            builder.Services.Configure<AnimateOptions>(options =>
            {
                options.Animation = Animations.FadeDown;
                options.Duration = TimeSpan.FromMilliseconds(1000);
            });
            builder.Services.AddScoped<IAdPhotoRepository, AdHttpPhotoRepository>();
            builder.Services.AddFileReaderService(o => o.UseWasmSharedBuffer = true);

            builder.Services.AddSingleton<NavigationUrlService>();

            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(CultureInfo.CurrentCulture.Name); 
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(CultureInfo.CurrentCulture.Name); 
             
            await builder.Build().RunAsync();
        }
    }
}
