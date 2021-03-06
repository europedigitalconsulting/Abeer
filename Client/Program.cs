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
using Abeer.Shared.Security;
using Blazor.Analytics;
using BlazorPro.BlazorSize;

namespace Abeer.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.RootComponents.Add<App>("app");

            Microsoft.Extensions.Configuration.IConfigurationRoot configurationRoot = builder.Configuration.Build();
            
            builder.Services.AddHttpClient(configurationRoot["Service:Api:AnonymousApiName"], client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));

            builder.Services.AddHttpClient(configurationRoot["Service:Api:ApiName"], client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

            // Supply HttpClient instances that include access tokens when making requests to the server project
            builder.Services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient(configurationRoot["Service:Api:ApiName"]));
            
            builder.Services.AddLocalization();
            builder.Services.AddApiAuthorization();
            builder.Services.Configure<AnimateOptions>(options =>
            {
                options.Animation = Animations.FadeDown;
                options.Duration = TimeSpan.FromMilliseconds(1000);
            });
            builder.Services.AddScoped<IAdPhotoRepository, AdHttpPhotoRepository>();
            builder.Services.AddFileReaderService(o => o.UseWasmSharedBuffer = true);
            builder.Services.AddAuthorizationCore(options => options.AddPolicy("OnlySubscribers",
                    policy => policy.Requirements.Add(new OnlySubscribersRequirement()))); 
            builder.Services.AddSingleton<NavigationUrlService>();
            builder.Services.AddCors((opt) => {
                opt.AddDefaultPolicy(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyMethod());
                });


            var gta = configurationRoot["Service:GoogleAnalytics:GTA"];
            builder.Services.AddGoogleAnalytics(gta, true);

            builder.Services.AddScoped<IResizeListener, ResizeListener>();
            builder.Services.AddScoped<IMediaQueryService, MediaQueryService>();

            await builder.Build().RunAsync();
        }
    }
}
