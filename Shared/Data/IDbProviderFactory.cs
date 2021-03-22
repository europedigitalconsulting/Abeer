using System;
using System.Collections.Concurrent;
using System.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Abeer.Shared.Data
{
    public interface IDbProviderFactory
    {
        IDbProvider GetProvider(string providerName);
        void RegisterProvider(IDbProvider dbProvider, string providerName);
        void RegisterProvider(Type providerType, string providerName);
    }

    public class DbProviderFactory : IDbProviderFactory
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly ConcurrentDictionary<string, IDbProvider> _providers =
            new ConcurrentDictionary<string, IDbProvider>();

        public DbProviderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IDbProvider GetProvider(string providerName)
        {
            return _providers.ContainsKey(providerName) ? _providers[providerName] : null;
        }

        public void RegisterProvider(IDbProvider dbProvider, string providerName)
        {
            if (_providers.ContainsKey(providerName)) throw new DuplicateNameException();
            _providers.TryAdd(providerName, dbProvider);
        }

        public void RegisterProvider(Type providerType, string providerName)
        {
            if (_providers.ContainsKey(providerName)) throw new DuplicateNameException();
            var dbProvider = (IDbProvider)_serviceProvider.GetService(providerType);
            _providers.TryAdd(providerName, dbProvider);
        }
    }
}