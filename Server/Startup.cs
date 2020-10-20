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
using Microsoft.Extensions.FileProviders;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Internal;

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
                    options.MigrationsAssembly(typeof(SecurityDbContext).Assembly.FullName)),ServiceLifetime.Transient);

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
            var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), @"StaticFiles");
            
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"StaticFiles")),
                RequestPath = new PathString("/StaticFiles")
            });

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
                var functionalDb = scope.ServiceProvider.GetRequiredService<FunctionalUnitOfWork>();

                db.Database.EnsureCreated();
                functionalDb.EnsureCreated();

                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                var admin = await userManager.FindByEmailAsync("admin@abeer.io");

                if (admin == null)
                {
                    admin = new ApplicationUser
                    {
                        UserName = "admin@abeer.io",
                        Country = "France",
                        DisplayName = "Michel Bruchet",
                        Email = "admin@abeer.io",
                        Title = "CEO",
                        Description = "Lorem ipsum dolor sit amet,consectetur adipiscing elit,sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum",
                        EmailConfirmed = true,
                        FirstName = "Michel",
                        LastName = "Bruchet",
                        City = "Quincy Sous senart",
                        PhoneNumber = "+33 7 80 81 10 24",
                        PinCode = "12345678901234567",
                        PinDigit = 12345,
                        IsAdmin = true
                    };

                    var addResult = await userManager.CreateAsync(admin, "Xc9wf8or&");

                    if (addResult.Succeeded)
                    {
                        await functionalDb.SocialNetworkRepository.AddSocialNetwork(new SocialNetwork { OwnerId = admin.Id, Name = "Facebook", Logo = "fab fa-facebook-square", DisplayInfo = "michel.bruchet", BackgroundColor = "bg-primary", Url = "https://www.facebook.com/michel.bruchet" });
                        await functionalDb.SocialNetworkRepository.AddSocialNetwork(new SocialNetwork { OwnerId = admin.Id, Name = "Instagram", Logo = "fab fa-instagram-square", DisplayInfo = "@michel.bruchet", BackgroundColor = "bg-danger", Url = "https://www.instagram.com" });
                        await functionalDb.SocialNetworkRepository.AddSocialNetwork(new SocialNetwork { OwnerId = admin.Id, Name = "Whatsapp", Logo = "fab fa-whatsapp-square", DisplayInfo = "+33 780811024", BackgroundColor = "bg-success", Url = "whatsapp:33780811024" });
                    }

                   
                }
                var hasan = await userManager.FindByEmailAsync("customer@abeer.io");
                if (hasan == null)
                {
                    hasan = new ApplicationUser
                    {
                        UserName = "customer@abeer.io",
                        Country = "France",
                        DisplayName = "Hasan Basri",
                        Email = "customer@abeer.io",
                        Title = "World's Master",
                        Description = "Lorem ipsum dolor sit amet,consectetur adipiscing elit,sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum",
                        EmailConfirmed = true,
                        FirstName = "Hasan",
                        LastName = "Basri",
                        City = "Paris",
                        PhoneNumber = "+66 624796927"
                    };

                    var addHasan = await userManager.CreateAsync(hasan, "Xc9wf8or&");

                    if (addHasan.Succeeded)
                    {
                        await functionalDb.SocialNetworkRepository.AddSocialNetwork(new SocialNetwork
                        {
                            OwnerId = hasan.Id,
                            Name = "Facebook",
                            Logo = "fab fa-facebook-square",
                            DisplayInfo = "hasan.basri",
                            BackgroundColor = "bg-primary",
                            Url = "https://www.facebook.com/hasan.basri"
                        });

                        await functionalDb.SocialNetworkRepository.AddSocialNetwork(new SocialNetwork
                        {
                            OwnerId = hasan.Id,
                            Name = "Instagram",
                            Logo = "fab fa-instagram-square",
                            DisplayInfo = "@hasan.basri",
                            BackgroundColor = "bg-danger",
                            Url = "https://www.instagram.com/hasan.basri"
                        });

                        await functionalDb.SocialNetworkRepository.AddSocialNetwork(new SocialNetwork
                        {
                            OwnerId = hasan.Id,
                            Name = "Whatsapp",
                            Logo = "fab fa-whatsapp-square",
                            DisplayInfo = "+66 624796927",
                            BackgroundColor = "bg-success",
                            Url = "whatsapp:66624796927"
                        });
                    }
                }
                var contact1 = await functionalDb.ContactRepository.Where(c  => c.OwnerId == admin.Id);
                if (!contact1.Any())
                    await functionalDb.ContactRepository.AddAsync(new Contact { OwnerId = admin.Id, UserId = hasan.Id });
                var contact2 = await functionalDb.ContactRepository.Where(c => c.OwnerId == hasan.Id);
                if (!contact2.Any())
                    await functionalDb.ContactRepository.AddAsync(new Contact { OwnerId = hasan.Id, UserId = admin.Id });
                await functionalDb.SaveChangesAsync();
            }
        }
    }
}
