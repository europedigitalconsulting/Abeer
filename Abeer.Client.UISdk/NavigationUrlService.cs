using Microsoft.AspNetCore.Components;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Abeer.Client.UISdk
{
    public class NavigationUrlService : INotifyPropertyChanged
    {
        private readonly NavigationManager navigationManager;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public NavigationUrlService(NavigationManager navigationManager)
        {
            this.navigationManager = navigationManager;
            ProfileUrl = navigationManager.ToAbsoluteUri("/profile").ToString();
            ImportContactUrl = navigationManager.ToAbsoluteUri("/addContact").ToString();
            ServicesUrl = navigationManager.ToAbsoluteUri("/services").ToString();
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
    }
}
