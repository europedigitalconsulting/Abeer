using Microsoft.AspNetCore.Components;

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Abeer.Client.UISdk
{
    public class NavigationUrlService : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public NavigationUrlService(NavigationManager navigationManager)
        {
            _ProfileUrl = navigationManager.ToAbsoluteUri("/profile").ToString();
            _ImportContactUrl = navigationManager.ToAbsoluteUri("/addContact").ToString();
            _ServicesUrl = navigationManager.ToAbsoluteUri("/services").ToString();
            _ContactsUrl = navigationManager.ToAbsoluteUri("/contacts").ToString();
            _MyAdsUrl = navigationManager.ToAbsoluteUri("/MyAds").ToString();
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

        string _MyAdsUrl;
        public string MyAdsUrl
        {
            get => _MyAdsUrl;
            set
            {
                _MyAdsUrl = value;
                OnPropertyChanged();
            }
        }
    }
}
