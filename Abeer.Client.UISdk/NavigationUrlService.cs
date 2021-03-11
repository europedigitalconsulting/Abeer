using Microsoft.AspNetCore.Components;

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Abeer.Client.UISdk
{
    public class NavigationUrlService : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _ShowMyAds;
        public bool ShowMyAds
        {
            get => _ShowMyAds;
            set
            {
                _ShowMyAds = value;
                OnPropertyChanged();
            }
        }

        private bool _ShowContacts;

        public bool ShowContacts 
        { 
            get => _ShowContacts; 
            set
            {
                _ShowContacts = value;
                OnPropertyChanged();
            }
        }

        private bool _ShowEditProfile;
        public bool ShowEditProfile { 
            get => _ShowEditProfile;
            set
            {
                _ShowEditProfile = value;
                OnPropertyChanged();
            }
        }

        private bool _ShowImport;
        
        public bool ShowImport 
        {
            get => _ShowImport;
            set
            {
                _ShowImport = value;
                OnPropertyChanged();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public NavigationUrlService(NavigationManager navigationManager)
        {
            _ProfileUrl = navigationManager.ToAbsoluteUri("/profile").ToString();
            _ImportContactUrl = navigationManager.ToAbsoluteUri("/contact/import").ToString();
            _ServicesUrl = navigationManager.ToAbsoluteUri("/services").ToString();
            _ContactsUrl = navigationManager.ToAbsoluteUri("/contacts/list").ToString();
            _MyAdsUrl = navigationManager.ToAbsoluteUri("/ads/myads").ToString();
            _ProfileEdit = navigationManager.ToAbsoluteUri("/Profile/Edit").ToString();
        }

        public void SetUrls(string mapUrl, string importContactUrl)
        {
            MapUrl = mapUrl;
            ImportContactUrl = importContactUrl;
        }

        private string _MapUrl;

        public string MapUrl
        {
            get => _MapUrl;
            set
            {
                _MapUrl = value;
                OnPropertyChanged();
            }
        }

        private string _ProfileUrl;
        public string ProfileUrl
        {
            get => _ProfileUrl;
            set
            {
                _ProfileUrl = value;
                OnPropertyChanged();
            }
        }

        private string _ProfileId;
        public string ProfileId
        {
            get => _ProfileId;
            set
            {
                _ProfileId = value;
                OnPropertyChanged();
            }
        }

        private string _ImportContactUrl;
        
        public string ImportContactUrl
        {
            get => _ImportContactUrl;
            set
            {
                _ImportContactUrl = value;
                OnPropertyChanged();
            }
        }

        private string _ContactsUrl;

        public string ContactsUrl
        {
            get => _ContactsUrl;
            set
            {
                _ContactsUrl = value;
                OnPropertyChanged();
            }
        }

        private string _ServicesUrl;
        public string ServicesUrl
        {
            get => _ServicesUrl;
            set
            {
                _ServicesUrl = value;
                OnPropertyChanged();
            }
        }

        private string _MyAdsUrl;
        public string MyAdsUrl
        {
            get => _MyAdsUrl;
            set
            {
                _MyAdsUrl = value;
                OnPropertyChanged();
            }
        }

        private string _ProfileEdit;

        public string ProfileEdit
        {
            get => _ProfileEdit;
            set
            {
                _ProfileEdit = value;
                OnPropertyChanged();
            }
        }
    }
}
