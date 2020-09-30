using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Abeer.Server.Data;
using System;
using Abeer.Shared;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using IdentityServer4.Services;
using Abeer.Data.UnitOfworks;
using Abeer.Services;
using Microsoft.EntityFrameworkCore;
using Abeer.Data;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Abeer.Data.Contextes;
using Abeer.Server.Hubs;

namespace Abeer.Server
{
    public class Startup
    {
        private CancellationTokenSource _cancellationTokenSource;

        public Startup(IConfiguration configuration, IHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IServiceProvider ServiceProvider { get; private set; }
        public IHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            AddContext<IFunctionalDbContext, FunctionalDbContext>(services, "FunctionalDbContextConnectionStrings");
            AddContext<ISecurityDbContext, SecurityDbContext>(services, "SecurityDbContextConnectionStrings");

            services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<SecurityDbContext>()
                .AddClaimsPrincipalFactory<UserClaimsPrincipalFactory>();

            services.AddIdentityServer()
                .AddApiAuthorization<ApplicationUser, SecurityDbContext>();

            services.AddAuthentication()
                .AddIdentityServerJwt();

            services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, UserClaimsPrincipalFactory>();
            services.AddTransient<IProfileService, ProfileService>();

            services.AddTransient<IEmailSenderService, EmailSenderFactory>();

            services.ConfigureApplicationCookie(o =>
            {
                o.ExpireTimeSpan = TimeSpan.FromDays(5);
                o.SlidingExpiration = true;
            });

            services.AddScoped<UrlShortner>();

            var mvcBuilder = services.AddControllersWithViews().AddNewtonsoftJson(options =>
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                );

            if (Environment.IsDevelopment())
            {
                mvcBuilder.AddRazorRuntimeCompilation();
            }

            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.AddRazorPages().AddMvcLocalization().AddViewLocalization();
            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("en"),
                    new CultureInfo("de"),
                    new CultureInfo("fr"),
                    new CultureInfo("es"),
                    new CultureInfo("ru"),
                    new CultureInfo("ja"),
                    new CultureInfo("ar"),
                    new CultureInfo("zh")
                };
                options.DefaultRequestCulture = new RequestCulture("fr");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            AddUnityOfWork<IFunctionalDbContext, FunctionalUnitOfWork>(services);
            AddUnityOfWork<ISecurityDbContext, SecurityUnitOfWork>(services);

            var templateProviderType = Configuration["EmailSender:MailTemplateProviderType"];

            services.AddScoped<ITemplateFileReader>(sp => new TemplateFileReader());

            services.AddScoped<CountriesService>();
            services.AddTransient<Task<DiscoveryDocumentResponse>>(sp =>
            {
                var log = sp.GetRequiredService<ILogger<Program>>();

                var client = new HttpClient();
                return client.GetDiscoveryDocumentAsync(Configuration["Service:STS:IdentityServerUrl"])
                    .ContinueWith(t =>
                    {
                        var disco = t.Result;

                        if (disco.IsError)
                        {
                            log.LogError($"get identityserver discovery  failed {disco.Error}");
                            throw new PlatformNotSupportedException(disco.Error);
                        }

                        log.LogInformation("get identityServer discovery successfully");

                        return disco;
                    });
            });

            services.AddTransient<Task<TokenResponse>>(sp =>
            {
                var client = new HttpClient();
                var log = sp.GetRequiredService<ILogger<Program>>();
                var discoHandler = sp.GetRequiredService<Task<DiscoveryDocumentResponse>>();

                return client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
                {
                    Address = discoHandler.Result.TokenEndpoint,
                    ClientId = Configuration["Service:STS:ClientId"],
                    ClientSecret = Configuration["Service:STS:ClientSecret"],
                    Scope = Configuration["Service:STS:Scopes"]
                }).ContinueWith(t =>
                {
                    var tokenResponse = t.Result;

                    if (tokenResponse.IsError)
                    {
                        log.LogError($"get identityserver discovery  failed {tokenResponse.Error}");
                        throw new PlatformNotSupportedException(tokenResponse.Error);
                    }

                    log.LogInformation("get identityServer token successfully");
                    return tokenResponse;
                });
            });
        }

        private void AddUnityOfWork<TDbContext, TUnityOfWork>(IServiceCollection services)
            where TUnityOfWork : class
        {
            services.AddScoped(sp => ActivatorUtilities.CreateInstance<TUnityOfWork>(sp, sp.GetRequiredService<TDbContext>()));
        }

        private void AddContext<TInterface, TService>(IServiceCollection services, string connectionStringsName)
            where TService : DbContext, TInterface
        {
            services.AddDbContext<TService>(options =>
                options.UseSqlite(
                    Configuration.GetConnectionString(connectionStringsName), options =>
                    options.MigrationsAssembly(typeof(SecurityDbContext).Assembly.FullName)));

            services.AddTransient(typeof(TInterface), sp =>
                ActivatorUtilities.GetServiceOrCreateInstance<TService>(sp));

            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            using var scope = app.ApplicationServices.CreateScope();

            SeedUserData(scope, env);
            SeedCountries(scope, env);

            app.UseMiddleware<UrlShortnerRewriter>();

            if (env.IsDevelopment() || env.EnvironmentName.Contains("Azure", StringComparison.OrdinalIgnoreCase))
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            //var rewriteUrlShortner = new RewriteOptions().AddRewrite(@"^\/shortned\/([0-9A-z-]+)", ")
            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseAuthorization();

            var localizationOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>().Value;
            app.UseRequestLocalization(localizationOptions);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
                endpoints.MapHub<SynchroHub>("/synchro");
            });
        }

        private void SeedCountries(IServiceScope scope, IWebHostEnvironment env)
        {
            var db = scope.ServiceProvider.GetRequiredService<FunctionalUnitOfWork>();
            db.EnsureCreated();

            var countriesService = scope.ServiceProvider
                .GetRequiredService<CountriesService>();

            countriesService.SeedData(env.WebRootPath).Wait();
        }

        private async void SeedUserData(IServiceScope scope, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                var db = scope.ServiceProvider.GetRequiredService<SecurityDbContext>();
                db.Database.EnsureCreated();

                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                var admin = await userManager.FindByEmailAsync("admin@Abeer.io");

                if (admin == null)
                {
                    //create admin
                    await userManager.CreateAsync(new ApplicationUser
                    {
                        UserName = "admin@crypocoin.io",
                        Country = "France",
                        DisplayName = "adminisrator",
                        Email = "admin@Abeer.io",
                        EmailConfirmed = true,
                        FirstName = "Admin",
                        LastName = "admin",
                        IsAdmin = true
                    }, "Xc9wf8or&");
                }

                var manager = await userManager.FindByEmailAsync("manager@Abeer.io");

                if (manager == null)
                {
                    //create manager
                    await userManager.CreateAsync(new ApplicationUser
                    {
                        UserName = "manager@crypocoin.io",
                        Country = "France",
                        DisplayName = "manager",
                        Email = "manager@Abeer.io",
                        EmailConfirmed = true,
                        FirstName = "Manager",
                        LastName = "Manager",
                        IsManager = true
                    }, "Xc9wf8or&");
                }

                var uoperator = await userManager.FindByEmailAsync("operator@Abeer.io");

                if (uoperator == null)
                {
                    //create operator
                    await userManager.CreateAsync(new ApplicationUser
                    {
                        UserName = "operator@crypocoin.io",
                        Country = "France",
                        DisplayName = "operator",
                        Email = "operator@Abeer.io",
                        EmailConfirmed = true,
                        FirstName = "Operator",
                        LastName = "Operator",
                        IsOperator = true,
                    }, "Xc9wf8or&");
                }

                var ucustomer = await userManager.FindByEmailAsync("customer@Abeer.io");

                if (ucustomer == null)
                {
                    //create operator
                    await userManager.CreateAsync(new ApplicationUser
                    {
                        UserName = "customer@crypocoin.io",
                        Country = "France",
                        DisplayName = "customer",
                        Email = "customer@Abeer.io",
                        EmailConfirmed = true,
                        FirstName = "customer",
                        LastName = "customer",
                    }, "Xc9wf8or&");
                }
            }
        }
    }
}
