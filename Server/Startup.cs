using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Abeer.Data;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Internal;
using DbProvider.LiteDbProvider;
using Abeer.Shared.Data;
using Abeer.Shared.Functional;
using System.Net;
using Abeer.Shared.Security;
using Microsoft.AspNetCore.Authorization;
using Abeer.Client;
using System.Collections.Generic;
using System.Linq;
using Abeer.Services.Data;
using Abeer.Server.APIFeatures.Hubs;
using Abeer.Shared.ReferentielTable;
using Microsoft.AspNetCore.ResponseCompression;
using Abeer.Shared.ViewModels;
using Blazor.Analytics;
using BlazorPro.BlazorSize;
using Microsoft.AspNetCore.SignalR;

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
            services.Configure<IISServerOptions>(options =>
            {
                options.AutomaticAuthentication = false;
            });

            var provider = Configuration["Service:Database:DbProvider"];

            services.AddDbContext<SecurityDbContext>(options =>
                options.UseSqlite(
                    Configuration.GetConnectionString("SecurityDbContextConnectionStrings"), options =>
                    options.MigrationsAssembly(typeof(SecurityDbContext).Assembly.FullName)), ServiceLifetime.Transient);

            services.AddTransient(sp =>
            {
                var provider = Configuration["Service:Database:DbProvider"];
                Type instanceType = Type.GetType(provider);
                return (IDbProvider)ActivatorUtilities.CreateInstance(sp, instanceType, Configuration);
            });


            services.AddTransient<FunctionalDbContext>();
            services.AddTransient<FunctionalUnitOfWork>();

            services.AddSignalR();
            services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();

            services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<SecurityDbContext>()
                .AddClaimsPrincipalFactory<UserClaimsPrincipalFactory>();

            services.AddIdentityServer()
                .AddApiAuthorization<ApplicationUser, SecurityDbContext>();

            services.AddSingleton<IAuthorizationHandler, OnlySubscribersRequirement>();
            services.AddAuthorization(options =>
            {
                options.AddPolicy("OnlySubscribers",
                    policy => policy.Requirements.Add(new OnlySubscribersRequirement()));
            });
            services.AddAuthentication()
                .AddIdentityServerJwt();

            services.AddCors();
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

            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(CultureInfo.CurrentCulture.Name);
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(CultureInfo.CurrentCulture.Name);

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

            services.AddTransient<NotificationService>();
            services.AddTransient<EventTrackingService>();

            services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseResponseCompression();

            using var scope = app.ApplicationServices.CreateScope();

            SeedAdPrices(scope, env).Wait();
            SeedNetworkSocials(scope, env);
            SeedUserData(scope, env).Wait();
            SeedCountries(scope, env);
            //SeedNotifications(scope, env).Wait();

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
            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod()
            );

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
                endpoints.MapHub<NotificationHub>("/notification");
            });
        }

        /*
        private async Task SeedNotifications(IServiceScope scope, IWebHostEnvironment env)
        {
            var dbSecurity = scope.ServiceProvider.GetRequiredService<SecurityDbContext>();
            var dbFunctional = scope.ServiceProvider.GetRequiredService<FunctionalUnitOfWork>();

            dbSecurity.Database.EnsureCreated();
            dbFunctional.EnsureCreated();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();

            var users = await userManager.Users.ToListAsync();
            
            foreach(var user in users)
            {
                var notification = await notificationService.GetNotification(user.Id, "welcome");
    
                if(notification == null)
                    await notificationService.Create(user.Id, "Welcome", "/subscription-pack", "alert-welcome", "alert-welcome", "alert-welcome", "welcome");
            }
        }
        */

        private async Task SeedAdPrices(IServiceScope scope, IWebHostEnvironment env)
        {
            var db = scope.ServiceProvider.GetRequiredService<FunctionalUnitOfWork>();
            db.EnsureCreated();
            db.SetTimeout(360);

            var prices = await db.AdPriceRepository.All();

            if (prices.FirstOrDefault(p => p.PriceName == "Free") == null)
            {
                await db.AdPriceRepository.Add(new AdPrice()
                {
                    Value = 0,
                    DisplayDuration = 2,
                    DelayToDisplay = 2,
                    Id = Guid.NewGuid(),
                    PriceName = "Free",
                    PriceDescription = "Free for 2 days"
                });
            }

            if (prices.FirstOrDefault(p => p.PriceName == "5Days") == null)
            {
                await db.AdPriceRepository.Add(new AdPrice()
                {
                    Value = 125,
                    DisplayDuration = 5,
                    DelayToDisplay = 2,
                    Id = Guid.NewGuid(),
                    PriceName = "5Days",
                    PriceDescription = "5Days_Description"
                });
            }

            if (prices.FirstOrDefault(p => p.PriceName == "Gold") == null)
            {
                await db.AdPriceRepository.Add(new AdPrice()
                {
                    Value = 450,
                    DisplayDuration = 30,
                    DelayToDisplay = 0,
                    Id = Guid.NewGuid(),
                    PriceName = "Gold",
                    PriceDescription = "Gold_Description"
                });
            }
        }

        private void SeedNetworkSocials(IServiceScope scope, IWebHostEnvironment env)
        {
            var db = scope.ServiceProvider.GetRequiredService<FunctionalUnitOfWork>();
            db.EnsureCreated();
            db.SetTimeout(360);

            var socialNetworks = db.SocialNetworkRepository.GetNetworks();

            if (socialNetworks.Count < 1)
            {
                var networks = new List<SocialNetwork>
                {
                    new SocialNetwork{OwnerId = "system", BackgroundColor = "bg-primary", Logo = "fab fa-facebook-square", Name="Facebook", Url="https://www.facebook.com/{0}"},
                    new SocialNetwork{OwnerId = "system", BackgroundColor = "bg-danger", Logo = "fab fa-youtube-square", Name="Youtube", Url="https://youtube.com/c/{0}"},
                    new SocialNetwork{OwnerId = "system", BackgroundColor = "bg-danger", Logo = "fab fa-instagram-square", Name="Instagram", Url="https://www.instagram.com/{0}"},
                    new SocialNetwork{OwnerId = "system", BackgroundColor = "bg-primary", Logo = "fab fa-twitter-square", Name="Twitter", Url="http://twitter.com/{0}"},
                    new SocialNetwork{OwnerId = "system", BackgroundColor = "bg-secondary", Logo = "fab fa-pinterest-square", Name="Pinterest", Url="http://www.pinterest.com/{0}"},
                    new SocialNetwork{OwnerId = "system", BackgroundColor = "bg-primary", Logo = "fab fa-linkedin", Name="Linkedin", Url="https://www.linkedin.com/in/{0}"},
                    new SocialNetwork{OwnerId = "system", BackgroundColor = "bg-yellow", Logo = "fab fa-snapchat", Name="Snapchat", Url="http://www.snapchat.com/add/{0}"},
                    new SocialNetwork{OwnerId = "system", BackgroundColor = "bg-success", Logo = "fab fa-tiktok", Name="TikTok", Url="http://vt.tiktok.com/{0}"},
                    new SocialNetwork{OwnerId = "system", BackgroundColor = "bg-success", Logo = "fab fa-whatsapp", Name="Whatsapp", Url="http://www.Whatsapp/{0}"}
                };

                db.SocialNetworkRepository.AddSocialNetworks(networks);
            }
        }

        private void SeedCountries(IServiceScope scope, IWebHostEnvironment env)
        {
            var db = scope.ServiceProvider.GetRequiredService<FunctionalUnitOfWork>();
            db.EnsureCreated();
            db.SetTimeout(360);

            var countriesService = scope.ServiceProvider
                .GetRequiredService<CountriesService>();

            countriesService.SeedData(env.WebRootPath);
        }

        private async Task SeedUserData(IServiceScope scope, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                var db = scope.ServiceProvider.GetRequiredService<SecurityDbContext>();
                var functionalDb = scope.ServiceProvider.GetRequiredService<FunctionalUnitOfWork>();

                db.Database.EnsureCreated();
                functionalDb.EnsureCreated();

                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                var subscriptionPack = await functionalDb.SubscriptionPackRepository.All();

                if (subscriptionPack == null || subscriptionPack.Count == 0)
                {
                    subscriptionPack.Add(await functionalDb.SubscriptionPackRepository.AddSubscriptionPack(new SubscriptionPack { Enable = true, Popuplar=true, Label = "Standard", Price = 85, Description = "StandardDescription", Duration = 1 }));
                    subscriptionPack.Add(await functionalDb.SubscriptionPackRepository.AddSubscriptionPack(new SubscriptionPack { Enable = true, Label = "Premium", Price = 124, Description = "PremiumDescription", Duration = 1 }));
                    subscriptionPack.Add(await functionalDb.SubscriptionPackRepository.AddSubscriptionPack(new SubscriptionPack { Enable = true, Label = "Yearly", Price = 1240, Description = "YearlyDescription", Duration = 12 }));
                    functionalDb.SaveChanges();
                }

                var michel = await userManager.FindByNameAsync("admin@meetag.co");

                if (michel == null)
                {
                    michel = new ApplicationUser
                    {
                        UserName = "admin@meetag.co",
                        Country = "France",
                        DisplayName = "Michel Bruchet",
                        Email = "admin@meetag.co",
                        Title = "CEO",
                        Description = "Lorem ipsum dolor sit amet,consectetur adipiscing elit,sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum",
                        EmailConfirmed = true,
                        FirstName = "Michel",
                        LastName = "Bruchet",
                        City = "Quincy Sous senart",
                        PhoneNumber = "+33 7 80 81 10 24",
                        PinDigit = "12345678901234567",
                        PinCode = 12345,
                        IsAdmin = true,
                        IsUnlimited = true
                    };

                    var addResult = await userManager.CreateAsync(michel, "Xc9wf8or&");

                    if (addResult.Succeeded)
                    {
                        await functionalDb.SocialNetworkRepository.AddSocialNetwork(
                            new SocialNetwork
                            {
                                OwnerId = michel.Id,
                                Name = "Facebook",
                                Logo = "fab fa-facebook-square",
                                DisplayInfo = "michel.bruchet",
                                BackgroundColor = "bg-primary",
                                Url = "https://www.facebook.com/michel.bruchet"
                            });

                        await functionalDb.SocialNetworkRepository.AddSocialNetwork(
                            new SocialNetwork
                            {
                                OwnerId = michel.Id,
                                Name = "Instagram",
                                Logo = "fab fa-instagram-square",
                                DisplayInfo = "@michel.bruchet",
                                BackgroundColor = "bg-danger",
                                Url = "https://www.instagram.com"
                            });
                        await functionalDb.SocialNetworkRepository.AddSocialNetwork(
                            new SocialNetwork
                            {
                                OwnerId = michel.Id,
                                Name = "Whatsapp",
                                Logo = "fab fa-whatsapp-square",
                                DisplayInfo = "+33 780811024",
                                BackgroundColor = "bg-success",
                                Url = "whatsapp:33780811024"
                            });
                    }

                    await functionalDb.CardRepository.Add(new Card
                    {
                        CardNumber = michel.PinDigit,
                        CardType = "nfc",
                        GeneratedBy = "System",
                        GeneratedDate = DateTime.UtcNow,
                        IsGenerated = true,
                        IsSold = true,
                        IsUsed = true,
                        IsProcessing = true,
                        PinCode = michel.PinCode.ToString(),
                        Quantity = 1,
                        SoldBy = "system",
                        SoldDate = DateTime.UtcNow
                    }, michel.Id);
                }

                var demo = await userManager.FindByNameAsync("demo@meetag.co");

                if (demo == null)
                {
                    demo = new ApplicationUser
                    {
                        UserName = "demo@meetag.co",
                        Country = "France",
                        DisplayName = "Demonstration 1",
                        Email = "demo@meetag.co",
                        Title = "Demo",
                        Description = "Lorem ipsum dolor sit amet,consectetur adipiscing elit,sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum",
                        EmailConfirmed = true,
                        FirstName = "Demo",
                        LastName = "Bruchet",
                        City = "Quincy Sous senart",
                        PhoneNumber = "+33 7 80 81 10 24",
                        PinDigit = "22345678901234567",
                        PinCode = 12345,
                        IsAdmin = true,
                        IsUnlimited = false, SubscriptionStartDate = DateTime.UtcNow.AddDays(-1),
                        SubscriptionEndDate = DateTime.UtcNow.AddDays(-1).AddDays(5)
                    };

                    var addResult = await userManager.CreateAsync(demo, "Xc9wf8or&");

                    if (addResult.Succeeded)
                    {
                        await functionalDb.SocialNetworkRepository.AddSocialNetwork(
                            new SocialNetwork
                            {
                                OwnerId = demo.Id,
                                Name = "Facebook",
                                Logo = "fab fa-facebook-square",
                                DisplayInfo = "demo",
                                BackgroundColor = "bg-primary",
                                Url = "https://www.facebook.com/demo"
                            });

                        await functionalDb.SocialNetworkRepository.AddSocialNetwork(
                            new SocialNetwork
                            {
                                OwnerId = demo.Id,
                                Name = "Instagram",
                                Logo = "fab fa-instagram-square",
                                DisplayInfo = "@demo",
                                BackgroundColor = "bg-danger",
                                Url = "https://www.instagram.com"
                            });
                        await functionalDb.SocialNetworkRepository.AddSocialNetwork(
                            new SocialNetwork
                            {
                                OwnerId = demo.Id,
                                Name = "Whatsapp",
                                Logo = "fab fa-whatsapp-square",
                                DisplayInfo = "+33 780811024",
                                BackgroundColor = "bg-success",
                                Url = "whatsapp:33780811024"
                            });
                    }

                    await functionalDb.CardRepository.Add(new Card
                    {
                        CardNumber = demo.PinDigit,
                        CardType = "nfc",
                        GeneratedBy = "System",
                        GeneratedDate = DateTime.UtcNow,
                        IsGenerated = true,
                        IsSold = true,
                        IsUsed = true,
                        IsProcessing = true,
                        PinCode = demo.PinCode.ToString(),
                        Quantity = 1,
                        SoldBy = "system",
                        SoldDate = DateTime.UtcNow
                    }, demo.Id);
                }
            }
        }
    }
}
