using System;
using Abeer.Shared.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Abeer.Ads.Data
{
    public static class RegisterModuleExtension
    {
        private const string configurationKey = "Service:Database:Ads";

        public static IServiceCollection RegisterAdsModule(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<AdsContext>();
            services.AddScoped<AdsUnitOfWork>();

            return services;
        }

        public static IDbProviderFactory InitializeAdsModule(this IDbProviderFactory dbProviderFactory, IServiceProvider  sp, IConfiguration configuration)
        {
            string providerType = configuration[$"{configurationKey}:DbProvider"];
            Type instanceType = Type.GetType(providerType);

            var dbServerOptions = new DbServerOptions
            {
                ConnectionString = configuration.GetConnectionString(configuration[$"{configurationKey}:ConnectionStrings"]),
                DbContextType = configuration[$"{configurationKey}:DbContextType"],
                EnableDetailedErrors = bool.TryParse(configuration[$"{configurationKey}:EnableDetailedErrors"], out var enableDetailedErrors) && enableDetailedErrors,
                EnableSensitiveDataLogging = bool.TryParse(configuration[$"{configurationKey}:EnabledSensitiveDataLogging"], out var enabledSensitiveDataLogging) && enabledSensitiveDataLogging,
                MaxBatchSize = int.TryParse(configuration[$"{configurationKey}:MaxBatchSize"], out var maxBatchSize) ? maxBatchSize : 100,
                MigrationAssemblyName = configuration[$"{configurationKey}:MigrationAssemblyName"]
            };
             
            var dbProvider = (IDbProvider)ActivatorUtilities.CreateInstance(sp, instanceType ?? throw new InvalidOperationException(), dbServerOptions);
            dbProviderFactory.RegisterProvider(dbProvider, "Ads");

            return dbProviderFactory;
        }
    }
}
