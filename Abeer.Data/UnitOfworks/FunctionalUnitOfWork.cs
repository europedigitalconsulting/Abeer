using Abeer.Data.Repositories;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
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
        private CustomLinkRepository _CustomLinkRepository;
        private AdRepository _AdRepository;
        private AdPriceRepository _AdPriceRepository;

        private FunctionalDbContext ApplicationDbContext { get; }
        private IServiceProvider ServiceProvider { get; }


        public FunctionalUnitOfWork(IServiceProvider serviceProvider, FunctionalDbContext applicationDbContext)
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

        public CustomLinkRepository CustomLinkRepository
        {
            get
            {
                if (_CustomLinkRepository == null)
                    _CustomLinkRepository = ActivatorUtilities.CreateInstance<CustomLinkRepository>(ServiceProvider);

                return _CustomLinkRepository;
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

        public AdRepository AdRepository
        {
            get
            {
                if(_AdRepository == null)
                    _AdRepository = ActivatorUtilities.CreateInstance<AdRepository>(ServiceProvider);

                return _AdRepository;
            }
        }


        public AdPriceRepository AdPriceRepository
        {
            get
            {
                if (_AdPriceRepository == null)
                    _AdPriceRepository = ActivatorUtilities.CreateInstance<AdPriceRepository>(ServiceProvider);

                return _AdPriceRepository;
            }
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


        public Task BulkUpdate<T>(IList<T> entities) where T : class
        {
            return Task.Run(() => ApplicationDbContext.BulkUpdate<T>(entities));
        }

        public void EnsureCreated()
        {
            ApplicationDbContext.EnsureCreated();
        }

        public void SetTimeout(int timeout)
        {
            ApplicationDbContext.SetTimeout(timeout);
        }
    }
}