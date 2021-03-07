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
        private CountriesRepository _CountriesRepository;
        private SocialNetworkRepository _SocialNetworkRepository;
        private CustomLinkRepository _CustomLinkRepository;
        private AdRepository _AdRepository;
        private AdPriceRepository _AdPriceRepository;
        private PaymentRepository _PaymentRepository;
        private SubscriptionPackRepository _SubscriptionPackRepository;
        private SubscriptionHistoryRepository _SubscriptionHistoryRepository;
        private NotificationRepository _NotificationRepository;
        private InvitationRepository _InvitationRepository;

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
         
        public CountriesRepository CountriesRepository
        {
            get
            {
                if (_CountriesRepository == null)
                    _CountriesRepository = ActivatorUtilities.CreateInstance<CountriesRepository>(ServiceProvider);

                return _CountriesRepository;
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
        public SubscriptionPackRepository SubscriptionPackRepository
        {
            get
            {
                if (_SubscriptionPackRepository == null)
                    _SubscriptionPackRepository = ActivatorUtilities.CreateInstance<SubscriptionPackRepository>(ServiceProvider);

                return _SubscriptionPackRepository;
            }
        }

        public SubscriptionHistoryRepository SubscriptionHistoryRepository
        {
            get
            {
                if (_SubscriptionHistoryRepository == null)
                    _SubscriptionHistoryRepository = ActivatorUtilities.CreateInstance<SubscriptionHistoryRepository>(ServiceProvider);

                return _SubscriptionHistoryRepository;
            }
        }

        public NotificationRepository NotificationRepository
        {
            get
            {
                if (_NotificationRepository == null)
                    _NotificationRepository = ActivatorUtilities.CreateInstance<NotificationRepository>(ServiceProvider);

                return _NotificationRepository;
            }
        }

        public InvitationRepository InvitationRepository
        {
            get
            {
                if (_InvitationRepository == null)
                    _InvitationRepository = ActivatorUtilities.CreateInstance<InvitationRepository>(ServiceProvider);

                return _InvitationRepository;
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