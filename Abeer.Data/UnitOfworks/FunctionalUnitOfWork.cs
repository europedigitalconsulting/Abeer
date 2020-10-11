using Abeer.Data.Repositories;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Abeer.Data.UnitOfworks
{
    public class FunctionalUnitOfWork
    {
        private CardRepository _CardRepository;
        private ContactRepository _ContactRepository;
        private PaymentRepository _PaymentRepository;
        private PurchaseRepository _PurchaseRepository;
        private WalletRepository _WalletRepository;
        private TokenBatchRepository _TokenBatchRepository;
        private TokenItemRepository _TokenItemRepository;
        private CountriesRepository _CountriesRepository;
        private SocialNetworkRepository _SocialNetworkRepository;

        private IFunctionalDbContext ApplicationDbContext { get; }
        private IServiceProvider ServiceProvider { get; }


        public FunctionalUnitOfWork(IServiceProvider serviceProvider, IFunctionalDbContext applicationDbContext)
        {
            ApplicationDbContext = applicationDbContext;
            ServiceProvider = serviceProvider;
        }

        public CardRepository CardRepository
        {
            get
            {
                if (_CardRepository == null)
                    _CardRepository = ActivatorUtilities.CreateInstance<CardRepository>(ServiceProvider);

                return _CardRepository;
            }
        }

        public ContactRepository ContactRepository
        {
            get
            {
                if (_ContactRepository == null)
                    _ContactRepository = ActivatorUtilities.CreateInstance<ContactRepository>(ServiceProvider);

                return _ContactRepository;
            }
        }

        public SocialNetworkRepository SocialNetworkRepository
        {
            get
            {
                if (_SocialNetworkRepository == null)
                    _SocialNetworkRepository = ActivatorUtilities.CreateInstance<SocialNetworkRepository>(ServiceProvider);

                return _SocialNetworkRepository;
            }
        }

        public PaymentRepository PaymentRepository
        {
            get
            {
                if (_PaymentRepository == null)
                    _PaymentRepository = ActivatorUtilities.CreateInstance<PaymentRepository>(ServiceProvider);

                return _PaymentRepository;
            }
        }

        public PurchaseRepository PurchaseRepository
        {
            get
            {
                if (_PurchaseRepository == null)
                    _PurchaseRepository = ActivatorUtilities.CreateInstance<PurchaseRepository>(ServiceProvider);

                return _PurchaseRepository;
            }
        }

        public void DetectChanges()
        {
            ApplicationDbContext.DetectChanges();
        }

        public WalletRepository WalletRepository
        {
            get
            {
                if (_WalletRepository == null)
                    _WalletRepository = ActivatorUtilities.CreateInstance<WalletRepository>(ServiceProvider);

                return _WalletRepository;
            }
        }
        public TokenBatchRepository TokenBatchRepository
        {
            get
            {
                if (_TokenBatchRepository == null)
                    _TokenBatchRepository = ActivatorUtilities.CreateInstance<TokenBatchRepository>(ServiceProvider);

                return _TokenBatchRepository;
            }
        }


        public TokenItemRepository TokenItemRepository
        {
            get
            {
                if (_TokenItemRepository == null)
                    _TokenItemRepository = ActivatorUtilities.CreateInstance<TokenItemRepository>(ServiceProvider);

                return _TokenItemRepository;
            }
        }

        public CountriesRepository CountriesRepository
        {
            get
            {
                if (_CountriesRepository == null)
                    _CountriesRepository = ActivatorUtilities.CreateInstance<CountriesRepository>(ServiceProvider);

                return _CountriesRepository;
            }
        }


        public int SaveChanges()
        {
            return ApplicationDbContext.SaveChange();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await ApplicationDbContext.SaveChangesAsync();
        }

        public Task BulkUpdateAsync<T>(IList<T> entities) where T : class
        {
            return ApplicationDbContext.BulkUpdateAsync<T>(entities);
        }

        public void EnsureCreated()
        {
            ApplicationDbContext.EnsureCreated();
        }
    }
}
