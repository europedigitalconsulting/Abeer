#region usings
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
using Abeer.Shared.Data;
using Abeer.Shared.Functional;
using Abeer.Shared.Security;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;
using Abeer.Services.Data;
using Abeer.Server.APIFeatures.Hubs;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SignalR;
using Abeer.Ads.Data;
using AutoMapper;
using Abeer.Server.APIFeatures.Jobs;
#endregion
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
            services.AddAutoMapper(typeof(Startup));

            services.Configure<IISServerOptions>(options =>
            {
                options.AutomaticAuthentication = false;
            });

            var provider = Configuration["Service:Database:Core:DbProvider"];

            RegisterSecurityDbContext(services);

            services.AddTransient(sp =>
            {
                var provider = Configuration["Service:Database:Core:DbProvider"];
                Type instanceType = Type.GetType(provider);
                return (IDbProvider)ActivatorUtilities.CreateInstance(sp, instanceType, Configuration);
            });

            services.RegisterAdsModule(Configuration);

            services.AddSingleton<IDbProviderFactory>(sp =>
            {
                return new DbProviderFactory(sp);
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

            services.AddHostedService<TrackAdPublishingService>();
        }

        private void RegisterSecurityDbContext(IServiceCollection services)
        {
            if (Configuration["IdentityServer:DataBase:DbType"].Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                services.AddDbContext<SecurityDbContext>(options =>
                    options.UseSqlServer(
                        Configuration.GetConnectionString(Configuration["IdentityServer:DataBase:ConnectionString"]), options =>
                        options.MigrationsAssembly(typeof(SecurityDbContext).Assembly.FullName)), ServiceLifetime.Transient);
            }
            else
            {
                services.AddDbContext<SecurityDbContext>(options =>
                    options.UseSqlite(
                        Configuration.GetConnectionString(Configuration["IdentityServer:DataBase:ConnectionString"]), options =>
                        options.MigrationsAssembly(typeof(SecurityDbContext).Assembly.FullName)), ServiceLifetime.Transient);
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseResponseCompression();

            using var scope = app.ApplicationServices.CreateScope();
            InitializeModuleProvider(scope, env);

            SeedAdPrices(scope, env).Wait();
            SeedNetworkSocials(scope, env);
            SeedUserData(scope, env).Wait();
            SeedCountries(scope, env);
            SeedArticles(scope, env);
            //SeedNotifications(scope, env).Wait();

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
                endpoints.MapHub<SynchroHub>("/synchro");
                endpoints.MapHub<NotificationHub>("/notification");
                endpoints.MapFallbackToPage("/_Host");
            });
        }

        private void SeedArticles(IServiceScope scope, IWebHostEnvironment env)
        {
            var db = scope.ServiceProvider.GetRequiredService<AdsContext>();

            db.EnsureCreated();
            db.SetTimeout(360);

            seedWorkFamily(db);
            SeedMode(db);
            seedCar(db);
            seedHouse(db);
            seedAnimal(db);
            seedMultimedia(db);
            seedServices(db);
            seedTools(db);
            seedOthers(db);
        }

        private void SeedMode(AdsContext db)
        {
            if(!db.Families.Any(f=>f.Code == "mode"))
            {
                var family = db.Families.Add(new Ads.Models.AdsFamily
                {
                    Code = "mode",
                    FamilyId = Guid.NewGuid(),
                    Label = "mode",
                    LabelSearch = "mode",
                    MetaDescription = "mode",
                    MetaKeywords = "mode",
                    MetaTitle = "mode"
                });

                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    CategoryId = Guid.NewGuid(),
                    Code = "chaus",
                    Label = "chaus",
                    MetaDescription = "chaus",
                    MetaKeywords = "chaus",
                    MetaTitle = "chaus",
                    Family = family
                });

                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    CategoryId = Guid.NewGuid(),
                    Code = "accessoires",
                    Label = "accessoires",
                    MetaDescription = "accessoires",
                    MetaKeywords = "accessoires",
                    MetaTitle = "accessoires",
                    Family = family
                });

                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    CategoryId = Guid.NewGuid(),
                    Code = "bagages",
                    Label = "bagages",
                    MetaDescription = "bagages",
                    MetaKeywords = "bagages",
                    MetaTitle = "bagages",
                    Family = family
                });

                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    CategoryId = Guid.NewGuid(),
                    Code = "montresbijoux",
                    Label = "montresbijoux",
                    MetaDescription = "montresbijoux",
                    MetaKeywords = "montresbijoux",
                    MetaTitle = "montresbijoux",
                    Family = family
                });

                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    CategoryId = Guid.NewGuid(),
                    Code = "equipementBebes",
                    Label = "equipementBebes",
                    MetaDescription = "equipementBebes",
                    MetaKeywords = "equipementBebes",
                    MetaTitle = "equipementBebes",
                    Family = family
                });

                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    CategoryId = Guid.NewGuid(),
                    Code = "vetementsBebes",
                    Label = "vetementsBebes",
                    MetaDescription = "vetementsBebes",
                    MetaKeywords = "vetementsBebes",
                    MetaTitle = "vetementsBebes",
                    Family = family
                });

                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    CategoryId = Guid.NewGuid(),
                    Code = "Luxes",
                    Label = "Luxes",
                    MetaDescription = "Luxes",
                    MetaKeywords = "Luxes",
                    MetaTitle = "Luxes",
                    Family = family
                });

                db.SaveChange();
            }
        }

        private static void seedOthers(AdsContext db)
        {
            if (db.Families.Any(f => f.Code == "other"))
            {
                var family = db.Families.Add(new Ads.Models.AdsFamily
                {
                    Code = "other",
                    FamilyId = Guid.NewGuid(),
                    Label = "other",
                    LabelSearch = "other",
                    MetaDescription = "other",
                    MetaKeywords = "other",
                    MetaTitle = "other"
                });

                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    Code = "other",
                    CategoryId = Guid.NewGuid(),
                    FamilyId = family.FamilyId,
                    Label = "other",
                    MetaDescription = "other",
                    MetaKeywords = "other",
                    MetaTitle = "other"
                });

                db.SaveChange();
            }
        }

        private static void seedTools(AdsContext db)
        {
            if (!db.Families.Any(f => f.Code == "tools"))
            {
                var family = db.Families.Add(new Ads.Models.AdsFamily
                {
                    Code = "tools",
                    FamilyId = Guid.NewGuid(),
                    Label = "tools",
                    LabelSearch = "tools",
                    MetaDescription = "tools",
                    MetaKeywords = "tools",
                    MetaTitle = "tools"
                });

                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    Code = "Agricole",
                    CategoryId = Guid.NewGuid(),
                    FamilyId = family.FamilyId,
                    Label = "Agricole",
                    MetaDescription = "Agricole",
                    MetaKeywords = "Agricole",
                    MetaTitle = "Agricole"
                });

                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    Code = "Transport",
                    CategoryId = Guid.NewGuid(),
                    FamilyId = family.FamilyId,
                    Label = "Transport",
                    MetaDescription = "Transport",
                    MetaKeywords = "Transport",
                    MetaTitle = "Transport"
                });

                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    Code = "BTP",
                    CategoryId = Guid.NewGuid(),
                    FamilyId = family.FamilyId,
                    Label = "BTP",
                    MetaDescription = "BTP",
                    MetaKeywords = "BTP",
                    MetaTitle = "BTP"
                });

                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    Code = "Outilage",
                    CategoryId = Guid.NewGuid(),
                    FamilyId = family.FamilyId,
                    Label = "Outilage",
                    MetaDescription = "Outilage",
                    MetaKeywords = "Outilage",
                    MetaTitle = "Outilage"
                });

                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    Code = "EquipementIndustriels",
                    CategoryId = Guid.NewGuid(),
                    FamilyId = family.FamilyId,
                    Label = "EquipementIndustriels",
                    MetaDescription = "EquipementIndustriels",
                    MetaKeywords = "EquipementIndustriels",
                    MetaTitle = "EquipementIndustriels"
                });

                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    Code = "RestaurationHotellerie",
                    CategoryId = Guid.NewGuid(),
                    FamilyId = family.FamilyId,
                    Label = "RestaurationHotellerie",
                    MetaDescription = "RestaurationHotellerie",
                    MetaKeywords = "RestaurationHotellerie",
                    MetaTitle = "RestaurationHotellerie"
                });

                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    Code = "Office",
                    CategoryId = Guid.NewGuid(),
                    FamilyId = family.FamilyId,
                    Label = "Office",
                    MetaDescription = "Office",
                    MetaKeywords = "Office",
                    MetaTitle = "Office"
                });

                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    Code = "Market",
                    CategoryId = Guid.NewGuid(),
                    FamilyId = family.FamilyId,
                    Label = "Market",
                    MetaDescription = "Market",
                    MetaKeywords = "Market",
                    MetaTitle = "Market"
                });

                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    Code = "Health",
                    CategoryId = Guid.NewGuid(),
                    FamilyId = family.FamilyId,
                    Label = "Health",
                    MetaDescription = "Health",
                    MetaKeywords = "Health",
                    MetaTitle = "Health"
                });

                db.SaveChange();
            }
        }

        private static void seedServices(AdsContext db)
        {
            if (!db.Families.Any(f => f.Code == "service"))
            {
                var family = db.Families.Add(new Ads.Models.AdsFamily
                {
                    Code = "service",
                    FamilyId = Guid.NewGuid(),
                    Label = "services",
                    LabelSearch = "services",
                    MetaDescription = "services",
                    MetaKeywords = "services",
                    MetaTitle = "services"
                });

                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    CategoryId = Guid.NewGuid(),
                    Code = "prestation",
                    FamilyId = family.FamilyId,
                    Label = "prestation",
                    MetaDescription = "prestation",
                    MetaKeywords = "prestation",
                    MetaTitle = "prestation"
                });

                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    CategoryId = Guid.NewGuid(),
                    Code = "billetterie",
                    FamilyId = family.FamilyId,
                    Label = "billetterie",
                    MetaDescription = "billetterie",
                    MetaKeywords = "billetterie",
                    MetaTitle = "billetterie"
                });

                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    CategoryId = Guid.NewGuid(),
                    Code = "evenement",
                    FamilyId = family.FamilyId,
                    Label = "evenement",
                    MetaDescription = "evenement",
                    MetaKeywords = "evenement",
                    MetaTitle = "evenement"
                });

                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    CategoryId = Guid.NewGuid(),
                    Code = "cours",
                    FamilyId = family.FamilyId,
                    Label = "cours",
                    MetaDescription = "cours",
                    MetaKeywords = "cours",
                    MetaTitle = "cours"
                });

                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    CategoryId = Guid.NewGuid(),
                    Code = "covoiturage",
                    FamilyId = family.FamilyId,
                    Label = "covoiturage",
                    MetaDescription = "covoiturage",
                    MetaKeywords = "covoiturage",
                    MetaTitle = "covoiturage"
                });

                db.SaveChange();

            }
        }

        private static void seedMultimedia(AdsContext db)
        {
            if (!db.Families.Any(f => f.Code == "multimedia"))
            {
                var family = db.Families.Add(new Ads.Models.AdsFamily
                {
                    Code = "multimedia",
                    FamilyId = Guid.NewGuid(),
                    Label = "multimedia",
                    LabelSearch = "multimedia",
                    MetaDescription = "multimedia",
                    MetaKeywords = "multimedia",
                    MetaTitle = "multimedia"
                });

                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    CategoryId = Guid.NewGuid(),
                    Code = "informatique",
                    Family = family,
                    Label = "informatique",
                    MetaDescription = "informatique",
                    MetaKeywords = "informatique",
                    MetaTitle = "informatique"
                });


                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    CategoryId = Guid.NewGuid(),
                    Code = "game",
                    Family = family,
                    Label = "game",
                    MetaDescription = "game",
                    MetaKeywords = "game",
                    MetaTitle = "game"
                });

                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    CategoryId = Guid.NewGuid(),
                    Code = "image",
                    Family = family,
                    Label = "image",
                    MetaDescription = "image",
                    MetaKeywords = "image",
                    MetaTitle = "image"
                });

                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    CategoryId = Guid.NewGuid(),
                    Code = "Telephonie",
                    Family = family,
                    Label = "Telephonie",
                    MetaDescription = "Telephonie",
                    MetaKeywords = "Telephonie",
                    MetaTitle = "Telephonie"
                });

                db.SaveChange();

            }
        }

        private static void seedAnimal(AdsContext db)
        {
            if (!db.Families.Any(f => f.Code == "animal"))
            {
                var family = db.Families.Add(new Ads.Models.AdsFamily
                {
                    Code = "animal",
                    FamilyId = Guid.NewGuid(),
                    Label = "animals",
                    LabelSearch = "animals",
                    MetaDescription = "animals",
                    MetaKeywords = "animals",
                    MetaTitle = "animals"
                });

                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    Family = family,
                    CategoryId = Guid.NewGuid(),
                    Code = "PetFood",
                    Label = "PetFood",
                    MetaDescription = "PetFood",
                    MetaKeywords = "PetFood",
                    MetaTitle = "PetFood"
                });

                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    Family = family,
                    CategoryId = Guid.NewGuid(),
                    Code = "PetAccessories",
                    Label = "PetAccessories",
                    MetaDescription = "PetAccessories",
                    MetaKeywords = "PetAccessories",
                    MetaTitle = "PetAccessories"
                });

                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    Family = family,
                    CategoryId = Guid.NewGuid(),
                    Code = "Pets",
                    Label = "Pets",
                    MetaDescription = "Pets",
                    MetaKeywords = "Pets",
                    MetaTitle = "Pets"
                });

                db.SaveChange();

            }
        }

        private static void seedHouse(AdsContext db)
        {
            if (!db.Families.Any(f => f.Code == "house"))
            {
                var family = db.Families.Add(new Ads.Models.AdsFamily
                {
                    Code = "house",
                    FamilyId = Guid.NewGuid(),
                    Label = "house",
                    LabelSearch = "house",
                    MetaDescription = "house",
                    MetaKeywords = "house",
                    MetaTitle = "house"
                });

                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    Family = family,
                    CategoryId = Guid.NewGuid(),
                    Label = "rentHouse",
                    Code = "rentHouse",
                    MetaDescription = "rentHouse",
                    MetaKeywords = "rentHouse",
                    MetaTitle = "rentHouse"
                });

                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    Family = family,
                    CategoryId = Guid.NewGuid(),
                    Label = "newHouse",
                    Code = "newHouse",
                    MetaDescription = "newHouse",
                    MetaKeywords = "newHouse",
                    MetaTitle = "newHouse"
                });

                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    Family = family,
                    CategoryId = Guid.NewGuid(),
                    Label = "oldHouse",
                    Code = "oldHouse",
                    MetaDescription = "oldHouse",
                    MetaKeywords = "oldHouse",
                    MetaTitle = "oldHouse"
                });

                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    Family = family,
                    CategoryId = Guid.NewGuid(),
                    Label = "OfficeHouse",
                    Code = "OfficeHouse",
                    MetaDescription = "OfficeHouse",
                    MetaKeywords = "OfficeHouse",
                    MetaTitle = "OfficeHouse"
                });

                db.SaveChange();
            }
        }

        private static void seedCar(AdsContext db)
        {
            if (!db.Families.Any(f => f.Code == "car"))
            {
                var family = db.Families.Add(new Ads.Models.AdsFamily
                {
                    Code = "car",
                    FamilyId = Guid.NewGuid(),
                    Label = "car",
                    LabelSearch = "car",
                    MetaDescription = "car",
                    MetaKeywords = "car",
                    MetaTitle = "car"
                });

                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    Family = family,
                    Code = "car",
                    Label = "car",
                    MetaDescription = "car",
                    MetaKeywords = "car",
                    MetaTitle = "car",
                    CategoryId = Guid.NewGuid()
                });


                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    Family = family,
                    Code = "moto",
                    Label = "moto",
                    MetaDescription = "moto",
                    MetaKeywords = "moto",
                    MetaTitle = "moto",
                    CategoryId = Guid.NewGuid()
                });

                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    Family = family,
                    Code = "caraving",
                    Label = "caraving",
                    MetaDescription = "caraving",
                    MetaKeywords = "caraving",
                    MetaTitle = "caraving",
                    CategoryId = Guid.NewGuid()
                });

                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    Family = family,
                    Code = "truck",
                    Label = "truck",
                    MetaDescription = "truck",
                    MetaKeywords = "truck",
                    MetaTitle = "truck",
                    CategoryId = Guid.NewGuid()
                });

                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    Family = family,
                    Code = "carAccessories",
                    Label = "carAccessories",
                    MetaDescription = "carAccessories",
                    MetaKeywords = "carAccessories",
                    MetaTitle = "carAccessories",
                    CategoryId = Guid.NewGuid()
                });

                db.SaveChange();

            }
        }

        private static void seedWorkFamily(AdsContext db)
        {
            if (!db.Families.Any(f => f.Code == "work"))
            {
                var family = db.Families.Add(new Ads.Models.AdsFamily
                {
                    Code = "work",
                    FamilyId = Guid.NewGuid(),
                    Label = "work",
                    LabelSearch = "Job",
                    MetaDescription = "work",
                    MetaKeywords = "work",
                    MetaTitle = "work"
                });

                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    CategoryId = Guid.NewGuid(),
                    Code = "fulltimejob",
                    Family = family,
                    Label = "fulltimejob",
                    MetaDescription = "fulltimejob",
                    MetaKeywords = "fulltimejob",
                    MetaTitle = "fulltimejob"
                });

                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    CategoryId = Guid.NewGuid(),
                    Code = "freelance",
                    Family = family,
                    Label = "freelance",
                    MetaDescription = "freelance",
                    MetaKeywords = "freelance",
                    MetaTitle = "freelance"
                });

                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    CategoryId = Guid.NewGuid(),
                    Code = "homework",
                    Family = family,
                    Label = "homework",
                    MetaDescription = "homework",
                    MetaKeywords = "homework",
                    MetaTitle = "homework"
                });

                db.Categories.Add(new Ads.Models.AdsCategory
                {
                    CategoryId = Guid.NewGuid(),
                    Code = "particularwork",
                    Family = family,
                    Label = "particularwork",
                    MetaDescription = "particularwork",
                    MetaKeywords = "particularwork",
                    MetaTitle = "particularwork"
                });

                db.SaveChange();

            }
        }

        private void InitializeModuleProvider(IServiceScope scope, IWebHostEnvironment env)
        {
            var x = scope.ServiceProvider.GetRequiredService<IDbProviderFactory>();
            x.InitializeAdsModule(scope.ServiceProvider, Configuration);
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
                    subscriptionPack.Add(await functionalDb.SubscriptionPackRepository.AddSubscriptionPack(new SubscriptionPack { Enable = true, Popuplar = true, Label = "Standard", Price = 0, Description = "StandardDescription", Duration = 1 }));
                    subscriptionPack.Add(await functionalDb.SubscriptionPackRepository.AddSubscriptionPack(new SubscriptionPack { Enable = true, Popuplar = true, Label = "Premium", Price = 150, Description = "PremiumDescription", Duration = 1 }));
                    subscriptionPack.Add(await functionalDb.SubscriptionPackRepository.AddSubscriptionPack(new SubscriptionPack { Enable = true, Label = "Business", Price = 250, Description = "BusinessDescription", Duration = 1 }));
                    functionalDb.SaveChanges();
                }

                var michel = await userManager.FindByNameAsync("admin@meetag.co");

                if (michel == null)
                {
                    michel = new ApplicationUser
                    {
                        UserName = "admin@meetag.co",
                        Country = "FR",
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
                        Country = "FR",
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
                        IsUnlimited = false,
                        SubscriptionStartDate = DateTime.UtcNow.AddDays(-1),
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
